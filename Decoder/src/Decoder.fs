namespace Decoder

open System
open System.Collections.Generic

open Microsoft.EntityFrameworkCore

open Newtonsoft.Json
open Chessie.ErrorHandling

open Struct
open PackedValue

open Helpers
open Conversions

open KiotlogDB
open Catalog
open Request

module Decoder =

    type Decoding = {
        Ctx : Context

        Device : Devices option
        Sensors : Sensors list option
        FmtString : string option

        Unpacked : PackedValue list option
        Data : Dictionary<string, float> option
    }

    let private strToByteArray channel =
        match channel with
        | "sigfox" -> byteArrayFromHexString
        | "lorawan" | "klsn" -> Convert.FromBase64String
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

    let private serializeData dtx =
        JsonConvert.SerializeObject(dtx.Data.Value, Formatting.None)

    let private getDevice devices dtx =
        let _, _, device = dtx.Ctx.TopicParts.Value
        match getDevice device devices with
        | Ok (x, _) -> ok { dtx with Device = Some x }
        | Bad msgs -> fail msgs

    let private getSortedSensors dtx =
        match getSortedSensors dtx.Device.Value with
        | Ok (x, _) -> ok { dtx with Sensors = Some (x |> Seq.toList) }
        | Bad msgs -> fail msgs

    let private getFormatString dtx =
        match getFormatString dtx.Device.Value with
        | Ok (x, _) -> ok { dtx with FmtString = Some x }
        | Bad msgs -> fail msgs

    let private unpackStruct dtx =
        let channel, _, device = dtx.Ctx.TopicParts.Value
        let payload = strToByteArray channel dtx.Ctx.PayloadRaw.Value
        match unpack dtx.FmtString.Value payload with
        | Ok (x, _) -> ok { dtx with Unpacked = Some x }
        | Bad msgs -> fail (sprintf "[%s]" device::msgs)

    let private convertFields dtx =
        match convertMeasures dtx.Sensors.Value dtx.Unpacked.Value with
        | Ok (x, _) -> ok { dtx with Data = Some x }
        | Bad msgs -> fail msgs

    let decodePayload (cs : string) ctx =

        let decoding = {
            Ctx = ctx
            Device = None
            Sensors = None
            FmtString = None
            Unpacked = None
            Data = None
        }

        let optionsBuilder = DbContextOptionsBuilder<KiotlogDBContext>()
        optionsBuilder.UseNpgsql(cs) |> ignore

        use dbCtx = new KiotlogDBContext(optionsBuilder.Options)
        let devices = getDevices dbCtx

        let decode =
            getDevice devices
            >> bind getSortedSensors
            >> bind getFormatString
            >> bind unpackStruct
            >> bind convertFields
            >> lift serializeData

        match decode decoding with
        | Ok (x, _) -> ok { ctx with Data = Some x }
        | Bad msgs -> fail (sprintf "Decoding failed: %A" msgs)
