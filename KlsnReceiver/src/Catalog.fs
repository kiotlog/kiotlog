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

namespace KlsnReceiver

open System
open Chessie.ErrorHandling
open Microsoft.EntityFrameworkCore

open KiotlogDBF.Context
open KiotlogDBF.Models

module Catalog =

    let getDevices (dbCtx : KiotlogDBFContext)  =
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

            // devices.SingleAsync(fun d -> d.Device = devId)
            // |> Async.AwaitTask
            // |> Async.RunSynchronously
            // |> ok
        with
            | :? InvalidOperationException as ex ->
                sprintf "Device %s not found. [%s]" devId ex.Message
                |> fail
