namespace Decoder

open System
open Newtonsoft.Json
open Newtonsoft.Json.Linq

open Chessie.ErrorHandling

open Helpers

module Request =

    type Context = {
        TopicParts : string * string * string
        Request : JObject option
        PayloadRaw : string option

        Datetime : DateTime option
        Flags : string option
        Data : string option
    }

    let validateMsg ctx (m : byte []) =
        try
            let json = m |> decode |> JObject.Parse
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
            let channel, _, _ = ctx.TopicParts
            let dateTime =
                match channel with
                | "sigfox" -> unixTimeStampToDateTime(int64 time)
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
        let now = DateTime.Now.ToUniversalTime()

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

            let _, _, device = x.TopicParts
            printfn "%s - [%A] [%s] %A" header now device msg

        let failure msgs =
            eprintfn "%s - [%A] ERRORS: %A" header now msgs
        
        eitherTee success failure twoTrackInput

    // let logDecode twoTrackInput =
    //     let now = DateTime.Now.ToUniversalTime()
        
    //     let success (x, _) =
    //         let data = x.Data.Value
    //         let _, _, device = x.TopicParts
    //         printfn "DECODING - [%A] [%s] %A" now device data

    //     let failure msgs =
    //         eprintfn "DECODING - [%A] ERRORS: %A" now msgs
        
    //     eitherTee success failure twoTrackInput

    // let logRequest twoTrackInput =
    //     let now = DateTime.Now.ToUniversalTime()

    //     let success (x, _) =
    //         let req = x.Request.Value.ToString(Formatting.None)
    //         let _, _, device = x.TopicParts
    //         printfn "REQUEST - [%A] [%s] %A" now device req

    //     let failure msgs =
    //         eprintfn "REQUEST - [%A] ERRORS %A" now msgs

    //     eitherTee success failure twoTrackInput

    let validateRequest c =
        validateMsg c
        >> bind validateMeta
        >> bind validateTime
        >> bind validatePayloadRaw
        >> log "request"
