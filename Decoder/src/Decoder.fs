namespace Decoder

open System

open Catalog
open Helpers
open Body
open Struct
open PackedValue
open KiotlogDB
open KlConversions
open System.Collections.Generic

module Decoder =

    let private doConvert (field : PackedValue, sensor : Sensors) =
        let max = sensor.SensorType.Meta.Max
        let min = sensor.SensorType.Meta.Min
        let fn = sensor.Conversion.Fun

        klConvert (field.ToFloat()) max min fn

    let private strToByteArray topic = 
        match topic with
        | "sigfox" -> byteArrayFromHexString
        | "lorawan" -> Convert.FromBase64String
        | _ -> byteArrayFromHexString

    let klDecode (cs : string) (channel, _, _) (msg : KlBody) =
        let formatString = getFormatString cs msg.DevId
        let sortedSensors = getSortedSensors cs msg.DevId |> Seq.toList

        let payload =
            msg.PayloadRaw
            |> strToByteArray channel
            |> unpack formatString
        
        // Replace with Dictionary<string, float>
        let decoded =
            let dict = new Dictionary<string, float>()

            List.zip payload sortedSensors
            |> List.iter (fun (p, s) -> dict.Add(s.Meta.Name, doConvert (p, s)))
            
            dict

        decoded
