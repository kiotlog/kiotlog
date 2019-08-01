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
open Newtonsoft.Json
open Newtonsoft.Json.Linq

open Chessie.ErrorHandling

open Helpers

module Request =

    type Context = {
        Msg : byte [] option
        TopicParts : (string * string * string) option

        Request : JObject option
        PayloadRaw : string option

        Datetime : DateTime option
        Flags : string option
        Data : string option
    }

    let validateMsg ctx =
        try
            let json = ctx.Msg.Value |> decode |> JObject.Parse
            ok { ctx with Request = Some json }
        with | :? JsonException -> fail "Invalid JSON"

    let validateMeta ctx =
        let flags =
            try
                ctx.Request.Value.["metadata"].ToString(Formatting.None) |> Some
            with | :? NullReferenceException -> None

        ok { ctx with Flags = flags }

    let validateTime ctx =
        let dateTime =
            match ctx.Flags with
            | Some _ ->
                let time = ctx.Request.Value.["metadata"].["time"] |> string
                let channel, _, _ = ctx.TopicParts.Value

                match String.IsNullOrEmpty time, channel with
                | false, "sigfox"
                | false, "klsn" | false, "klsnts" -> unixTimeStampToDateTime(int64 time)
                | false, "lorawan" ->
                    try
                        DateTime.Parse(time).ToUniversalTime()
                    with
                    | :? FormatException -> DateTime.UtcNow
                | _, _ -> DateTime.UtcNow
            | None -> DateTime.UtcNow

        ok { ctx with Datetime = Some dateTime}

    let validatePayloadRaw ctx =
        try
            let payload = ctx.Request.Value.["payload_raw"] |> string
            ok { ctx with PayloadRaw = Some payload}
        with | :? JsonException -> fail "Payload not found"

    let log what twoTrackInput =
        let now = DateTime.Now.ToUniversalTime().ToString("o")

        let header =
            match what with
            | "decode" -> "DECODE"
            | "request" -> "REQUEST"
            | _ -> "NONE"

        let success (x, _) =
            let msg =
                match what with
                | "decode" -> x.Data.Value
                | "request" -> x.Request.Value.ToString(Formatting.None)
                | _ -> "Hello, World!"

            let _, _, device = x.TopicParts.Value
            printfn "%s - [%s] [%s] %A" header now device msg

        let failure msgs =
            eprintfn "%s - [%s] ERRORS: %A" header now msgs

        eitherTee success failure twoTrackInput

    let validateRequest =
        validateMsg
        >> bind validateMeta
        >> bind validateTime
        >> bind validatePayloadRaw
        >> log "request"
