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
open System.Collections.Generic

open Microsoft.EntityFrameworkCore

open Chessie.ErrorHandling

open KiotlogDBF.Context
open KiotlogDBF.Models

module Catalog =

    let getDevices (dbCtx : KiotlogDBFContext)  =
        dbCtx.Devices
            .Include(fun d -> d.Sensors :> IEnumerable<Sensors>)
                .ThenInclude( fun (s : Sensors) -> s.SensorType)
            .Include(fun d -> d.Sensors :> IEnumerable<Sensors>)
                .ThenInclude( fun (s : Sensors) -> s.Conversion)

    let getDevice devId (devices : Linq.IQueryable<Devices>)  =
        try
            // query {
            //     for d in devices do
            //     where (d.Device = devId)
            //     select d
            //     exactlyOne
            // } |> ok

            devices.SingleAsync(fun d -> d.Device = devId)
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> ok
        with
            | :? AggregateException
            | :? InvalidOperationException as ex ->
                sprintf "Device %s not found. [%s]" devId ex.Message
                |> fail
