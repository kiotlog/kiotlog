namespace Decoder

open System
open System.Collections.Generic

open Chessie.ErrorHandling

open Struct
open PackedValue

open Helpers
open Conversions

open KiotlogDB
open Catalog
open Request
open Json

module Decoder =

    let private strToByteArray channel = 
        match channel with
        | "sigfox" -> byteArrayFromHexString
        | "lorawan" -> Convert.FromBase64String
        | _ -> encode

    let private doConvert (field : PackedValue) (sensor : Sensors) =
        let max = sensor.SensorType.Meta.Max
        let min = sensor.SensorType.Meta.Min
        let fn = sensor.Conversion.Fun

        klConvert (field.ToFloat()) max min fn

    let private convertMeasures sensors payload =
        match payload with
        | [] -> fail "Empty Payload"
        | _ ->
            let decodedDict = new Dictionary<string, float>()            
            List.iter2
                (fun (s : Sensors) p ->
                    decodedDict.[s.Meta.Name] <- doConvert p s)
                sensors payload 

            ok decodedDict

    let private decodePayload channel rawPayload device =
        let sortedSensors =
            getSortedSensors device
            |> returnOrFail
            |> Seq.toList

        let formatString =
            getFormatString device
            |> returnOrFail
        
        let payload =
            rawPayload
            |> strToByteArray channel

        let validatedUnpack p =
            unpack formatString p
            |> bind (convertMeasures sortedSensors)
        
        payload
        |> validatedUnpack
    
    let klDecode (cs : string) ctx =

        let channel, _, device = ctx.TopicParts

        use dbCtx = new KiotlogDBContext(cs)
        let devices = getDevices dbCtx

        let serializeData d =
            ok (SnakeCaseSerializer.serialize<Dictionary<string, float>> d)

        let validatedDecode p =        
            getDevice device devices
            |> bind (decodePayload channel p)
        
        let data =
            let success (x, _) = x
            let failure msgs = eprintfn "%A" msgs; ""

            ctx.PayloadRaw.Value
            |> validatedDecode
            |> bind serializeData
            |> either success failure
        
        ok { ctx with Data = Some data }
