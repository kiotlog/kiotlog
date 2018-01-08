namespace KlsnReceiver

open System
open Chessie.ErrorHandling
open Microsoft.EntityFrameworkCore

open KiotlogDB

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

            // devices.SingleAsync(fun d -> d.Device = devId)
            // |> Async.AwaitTask
            // |> Async.RunSynchronously
            // |> ok
        with
            | :? InvalidOperationException as ex ->
                sprintf "Device %s not found. [%s]" devId ex.Message
                |> fail
