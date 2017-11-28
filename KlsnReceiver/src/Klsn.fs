namespace KlsnReceiver

open System
open Chessie.ErrorHandling

open Sodium
open KiotlogDB
open Catalog
open SnPacket

module Klsn =

    type KlsnRequest =
        {
            TopicParts : ( string * string * string ) option
            Msg : byte [] option
            Device : Devices option
            Key : byte [] option
            Packet : SnPacket option
            Time : DateTime option
            Payload : byte [] option
        }

    let log what twoTrackInput =
        let now = DateTime.Now.ToUniversalTime()

        let header =
            match what with
            | "payload" -> "PAYLOAD"
            | "request" -> "REQUEST"
            | _ -> "NONE"

        let success (x, _) =
            let msg =
                match what with
                | "payload" -> x.Payload.Value |> Convert.ToBase64String
                | "request" -> x.ToString()
                | _ -> "Hello, World!"

            let _, _, device = x.TopicParts.Value
            printfn "%s - [%A] [%s] %A" header now device msg

        let failure msgs =
            eprintfn "%s - [%A] ERRORS: %A" header now msgs

        eitherTee success failure twoTrackInput

    let parseRequest cs ctx =
        use dbCtx = new KiotlogDBContext(cs)

        let devices = getDevices dbCtx

        let getDevice req =
            let _ ,_, device = req.TopicParts.Value
            match getDevice device devices with
            | Ok (x, _) -> ok { req with Device = Some x }
            | Bad msgs -> fail ( sprintf "%A" msgs)

        let getKey req =
            try
                let key = req.Device.Value.Auth.Klsn.Key |> Convert.FromBase64String
                ok { req with Key = Some key }
            with
                | _ -> fail "Key not found"

        let parseMsg req =
            try
                let packet = parseSnPacket req.Msg.Value
                ok { req with Packet = Some packet }
            with
                | :? InvalidOperationException as ex ->
                    sprintf "MsgPack Deserialization failed : %s" ex.Message |> fail

        let tryDecrypt req =
            let data, nonce, key =
                req.Packet.Value.Data,
                req.Packet.Value.Nonce,
                req.Key.Value
            try
                let plain = SecretAeadIETF.Decrypt(data, nonce, key)
                ok { req with Payload = Some plain }
            with
                | _ -> fail "AEAD Failed"

        let validateRequest =
            getDevice
            >> bind getKey
            >> bind parseMsg
            >> bind tryDecrypt

        ctx
        |> validateRequest