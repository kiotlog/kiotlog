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
            sensors
            |> Seq.map (fun (sensor : Sensors) -> sensor.Fmt.FmtChr)
            |> Seq.reduce (+)
            |> ok

        let buildFmtString endianness fmt =
            endianness + fmt |> ok

        let validateFmtString =
            getSortedSensors
            >> bind fmtString
            >> bind (buildFmtString endianness)

        validateFmtString device

    let writePoint cs (ctx : Context, _) =

        let _, _, device = ctx.TopicParts.Value
        use dbCtx = new KiotlogDBContext(cs)

        Points (
            DeviceDevice = device,
            Time = ctx.Datetime.Value,
            Flags = ctx.Flags.Value,
            Data = ctx.Data.Value )
        |> dbCtx.Points.Add |> ignore

        dbCtx.SaveChanges() |> ignore
