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
        try
            let flags = ctx.Request.Value.["metadata"].ToString(Formatting.None)
            ok { ctx with Flags = Some flags }
        with | :? NullReferenceException -> fail "Metadata not found"

    let validateTime ctx =
        try
            let time = ctx.Request.Value.["metadata"].["time"] |> string
            let channel, _, _ = ctx.TopicParts.Value
            let dateTime =
                match channel with
                | "sigfox" | "klsn" -> unixTimeStampToDateTime(int64 time)
                | "lorawan" -> DateTime.Parse(time).ToUniversalTime()
                | _ -> DateTime.UtcNow
            ok { ctx with Datetime = Some dateTime}
        with | _ -> fail "Invalid time"

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
