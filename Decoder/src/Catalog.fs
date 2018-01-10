(*
    Copyright (C) 2017 Giampaolo Mancini, Trampoline SRL.
    Copyright (C) 2017 Francesco Varano, Trampoline SRL.

    This file is part of Kiotlog.

    Kiotlog is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Kiotlog is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*)

namespace Decoder

open System
open Microsoft.EntityFrameworkCore

open Chessie.ErrorHandling

open KiotlogDB
open Request

module Catalog =

    let getDevices (dbCtx : KiotlogDBContext)  =
        dbCtx.Devices
            .Include("Sensors")
            .Include("Sensors.SensorType")
            .Include("Sensors.Conversion")

    let getDevice devId (devices : Linq.IQueryable<Devices>)  =
        try
            query {
                for d in devices do
                where (d.Device = devId)
                select d
                exactlyOne
            } |> ok
        with
            | :? InvalidOperationException as ex ->
                sprintf "Device %s not found. [%s]" devId ex.Message
                |> fail

    let getSortedSensors (device : Devices) =
        try
            device.Sensors
            |> Seq.sortBy (fun sensor -> sensor.Fmt.Index) |> ok
        with
            | :? ArgumentException as ex ->
                sprintf "Sensors not found for %s. [%s]" device.Device ex.Message
                |> fail

    let getFormatString (device : Devices) =
        let endianness =
            try
                if device.Frame.Bigendian then ">" else "<"
            with
                | :? ArgumentException as ex ->
                    eprintfn
                        "Endianness not specified for %s. Forcing Little. [%s]"
                        device.Device ex.Message
                    "<"

        let fmtString sensors =
            try
                sensors
                |> Seq.map (fun (sensor : Sensors) -> sensor.Fmt.FmtChr)
                |> Seq.reduce (+)
                |> ok
            with
                | :? ArgumentException as ex ->
                    sprintf "Unable to get format string for %s. [%s]" device.Device ex.Message
                    |> fail

        let buildFmtString endianness fmt =
            endianness + fmt |> ok

        let validateFmtString =
            getSortedSensors
            >> bind fmtString
            >> bind (buildFmtString endianness)

        validateFmtString device

    let writePoint (cs : string) (ctx : Context, _) =

        let _, _, device = ctx.TopicParts.Value

        let optionsBuilder = DbContextOptionsBuilder<KiotlogDBContext>()
        optionsBuilder.UseNpgsql(cs) |> ignore

        use dbCtx = new KiotlogDBContext(optionsBuilder.Options)

        // TODO: check if device was not found
        let devId = query {
                        for d in dbCtx.Devices do
                        where (d.Device = device)
                        select d.Id
                        exactlyOne
                    }

        Points (
            DeviceId = devId,
            Time = ctx.Datetime.Value,
            Flags = ctx.Flags.Value,
            Data = ctx.Data.Value )
        |> dbCtx.Points.Add |> ignore

        dbCtx.SaveChanges() |> ignore
