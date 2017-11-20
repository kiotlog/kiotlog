namespace Decoder

open Microsoft.EntityFrameworkCore

open KiotlogDB
open System
open System.Collections.Generic

open Helpers
open Struct
open PackedValue
open Conversions

module Catalog =

    let private doConvert (field : PackedValue) (sensor : Sensors) =
        let max = sensor.SensorType.Meta.Max
        let min = sensor.SensorType.Meta.Min
        let fn = sensor.Conversion.Fun

        klConvert (field.ToFloat()) max min fn

    let private strToByteArray topic = 
        match topic with
        | "sigfox" -> byteArrayFromHexString
        | "lorawan" -> Convert.FromBase64String
        | _ -> byteArrayFromHexString
 

    let private getDevices (ctx : KiotlogDBContext)  =

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
                | :? InvalidOperationException -> None
        device
       

    let private getSortedSensors (device : Devices) =  
        
        device.Sensors
        |> Seq.sortBy (fun sensor -> sensor.Fmt.Index)

    let private getFormatString (device : Devices) =

        let endianness = if device.Frame.Bigendian then ">" else "<"

        let fmtString =
            device
            |> getSortedSensors
            |> Seq.map (fun sensor -> sensor.Fmt.FmtChr)
            |> Seq.reduce (+)
        
        endianness + fmtString
    
    let klDecode (cs : string) (channel, _, _) devId payloadRaw : Dictionary<string, float> option =

        use ctx = new KiotlogDBContext(cs)
        
        ctx
        |> getDevices
        |> getDevice devId
        |> function
        | None -> None
        | Some device -> 
            let payload =
                payloadRaw
                |> strToByteArray channel
                |> unpack (getFormatString device)
            
            let sortedSensors = getSortedSensors device |> Seq.toList       
            let decodedDict = new Dictionary<string, float>()

            match payload with
            | [] -> None
            | _ ->
                List.iter2
                    (fun p (s : Sensors) ->
                        decodedDict.[s.Meta.Name] <- doConvert p s)
                    payload sortedSensors
    
                Some decodedDict

    let klWrite cs (device, time, flags, data) =
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