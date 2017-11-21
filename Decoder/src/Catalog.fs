namespace Decoder

open Microsoft.EntityFrameworkCore

open KiotlogDB
open System

module Catalog =

    let getDevices (ctx : KiotlogDBContext)  =
        ctx.Devices
            .Include("Sensors")
            .Include("Sensors.SensorType")
            .Include("Sensors.Conversion")
    
    let getDevice devId (devices : Linq.IQueryable<Devices>)  =
        let device =
            try
                query {
                    for d in devices do
                    where (d.Device = devId)
                    select d
                    exactlyOne
                } |> Some
            with
                | :? InvalidOperationException as ex ->
                    printfn "Device %s not found: %s" devId ex.Message
                    None
        device
       
    let getSortedSensors (device : Devices) =
        try 
            device.Sensors
            |> Seq.sortBy (fun sensor -> sensor.Fmt.Index) |> Some
        with
            | :? ArgumentException -> None

    let getFormatString (device : Devices) =
        let endianness = 
            try
                if device.Frame.Bigendian then ">" else "<"
            with
                | :? ArgumentException -> "<"

        let fmtString =
            device
            |> getSortedSensors
            |> function
            | None -> None
            | Some sensors ->
                sensors
                |> Seq.map (fun sensor -> sensor.Fmt.FmtChr)
                |> Seq.reduce (+)
                |> Some

        match fmtString with
        | None -> None
        | Some fmt -> endianness + fmt |> Some

    let writePoint cs (device, time, flags, data) =
        match data with
        | None -> ()
        | Some data ->
            use ctx = new KiotlogDBContext(cs)

            Points (
                DeviceDevice = device,
                Time = time,
                Flags = flags,
                Data = data )
            |> ctx.Points.Add |> ignore
        
            ctx.SaveChanges() |> ignore