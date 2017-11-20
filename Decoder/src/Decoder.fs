namespace Decoder

open System
open System.Collections.Generic

open Struct
open PackedValue

open Helpers
open Conversions

open KiotlogDB
open Catalog

module Decoder =

    let private strToByteArray topic = 
        match topic with
        | "sigfox" -> byteArrayFromHexString
        | "lorawan" -> Convert.FromBase64String
        | _ -> byteArrayFromHexString

    let private doConvert (field : PackedValue) (sensor : Sensors) =
        let max = sensor.SensorType.Meta.Max
        let min = sensor.SensorType.Meta.Min
        let fn = sensor.Conversion.Fun

        klConvert (field.ToFloat()) max min fn

    let private convertMeasures sensors =
        function
        | [] -> None
        | payload ->
            let decodedDict = new Dictionary<string, float>()            
            List.iter2
                (fun (s : Sensors) p ->
                    decodedDict.[s.Meta.Name] <- doConvert p s)
                sensors payload 

            Some decodedDict

    let private decodePayload channel rawPayload =
        function
        | None -> None
        | Some (device : Devices) -> 
            let sortedSensors = getSortedSensors device |> Seq.toList  
            let formatString = getFormatString device
            
            rawPayload
            |> strToByteArray channel
            |> unpack formatString
            |> convertMeasures sortedSensors
    
    let klDecode (cs : string) (channel, _, device) payloadRaw : Dictionary<string, float> option =

        use ctx = new KiotlogDBContext(cs)
        
        ctx
        |> getDevices
        |> getDevice device
        |> decodePayload channel payloadRaw

