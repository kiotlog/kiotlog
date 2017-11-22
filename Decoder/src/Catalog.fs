namespace Decoder

open Microsoft.EntityFrameworkCore

open Chessie.ErrorHandling

open KiotlogDB
open System

module Catalog =

    let getDevices (ctx : KiotlogDBContext)  =
        ctx.Devices
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

    let writePoint cs device time flags (data, _) =
        use ctx = new KiotlogDBContext(cs)

        Points (
            DeviceDevice = device,
            Time = time,
            Flags = flags,
            Data = data )
        |> ctx.Points.Add |> ignore
    
        ctx.SaveChanges() |> ignore