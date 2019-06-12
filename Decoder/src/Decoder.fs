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

namespace Decoder

open System
open System.Collections.Generic

open Newtonsoft.Json
open Chessie.ErrorHandling

open Decoder.Struct
open Decoder.PackedValue

open Decoder.Helpers
open Decoder.Conversions

open KiotlogDBF.Context
open KiotlogDBF.Models
open Decoder.Catalog
open Decoder.Request

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

        klConvert (field.ToDouble()) max min fn

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

    let decodePayload options ctx =

        let decoding = {
            Ctx = ctx
            Device = None
            Sensors = None
            FmtString = None
            Unpacked = None
            Data = None
        }

        use dbCtx = new KiotlogDBFContext(options)
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
