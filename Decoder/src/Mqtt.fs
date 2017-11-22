namespace Decoder

// open uPLibrary.Networking.M2Mqtt.Session
// open uPLibrary.Networking.M2Mqtt.Internal
// open uPLibrary.Networking.M2Mqtt.Exceptions
// open System.Threading
// open NGeoHash
// open Units

open System
open System.Threading
open System.Text.RegularExpressions
open System.Collections.Generic

open Microsoft.FSharpLu.Json

open uPLibrary.Networking.M2Mqtt
open uPLibrary.Networking.M2Mqtt.Messages
open uPLibrary.Networking.M2Mqtt.Exceptions

open Newtonsoft.Json
open Newtonsoft.Json.Linq

open Chessie.ErrorHandling

open Decoder
open Json
open Helpers


module Mqtt =

    type RequestPoint = {
        Json : JObject option
        Device : string option
        Datetime : DateTime option
        Flags : string option
        PayloadRaw: string option
    }

    let msgReceivedHandler decodeData writeData (e: MqttMsgPublishEventArgs) =

        let channel, app, device =
            let m = Regex(@"/(\S+)/(\S+)/devices/(\S+)/up").Match(e.Topic)
            if m.Success
                then m.Groups.[1].Value, m.Groups.[2].Value, m.Groups.[3].Value
                else ("", "", "")

        let point = {
            Json = None
            Device = Some device
            Datetime = None
            Flags = None
            PayloadRaw = None
        }

        let getMsg mm (m : byte []) =
            try
                let json = m |> decode |> JObject.Parse
                ok { mm with Json = Some json }
            with | :? JsonException -> fail "Invalid JSON"

        let getMeta mm =
            try
                let flags = mm.Json.Value.["metadata"].ToString(Formatting.None)
                ok { mm with Flags = Some flags }
            with | :? NullReferenceException -> fail "Metadata not found"

        let getTime mm =
            try
                let time = mm.Json.Value.["metadata"].["time"] |> string
                let dateTime = 
                    match channel with
                    | "sigfox" -> unixTimeStampToDateTime(int64 time)
                    | "lorawan" -> DateTime.Parse(time).ToUniversalTime()
                    | _ -> DateTime.UtcNow
                ok { mm with Datetime = Some dateTime}
            with | _ -> fail "Invalid time"

        let getPayloadRaw mm =
            try
                let payload = mm.Json.Value.["payload_raw"] |> string
                ok { mm with PayloadRaw = Some payload}
            with | :? JsonException -> fail "Payload not found"            

        let logDecode d twoTrackInput = 
            let success (x, _) = printfn "DECODING - [%A] [%s] %A" DateTime.Now d x
            let failure msgs = eprintfn "DECODING - [%A] [%s] ERRORS: %A" DateTime.Now d msgs
            eitherTee success failure twoTrackInput

        let logRequest d twoTrackInput = 
            let success (x, _) = printfn "REQUEST - [%A] [%s] %A" DateTime.Now d (x.Json.Value.ToString(Formatting.None))
            let failure msgs = eprintfn "REQUEST - [%A] [%s] ERRORS %A" DateTime.Now d msgs
            eitherTee success failure twoTrackInput

        let validateRequest p =
            getMsg p
            >> bind getMeta
            >> bind getTime
            >> bind getPayloadRaw
            >> logRequest device

        let validatedWrite mm =
            mm.PayloadRaw.Value
            |> decodeData (channel, app, device)
            |> bind (SnakeCaseSerializer.serialize<Dictionary<string, float>> >> ok)
            |> successTee (writeData device mm.Datetime.Value mm.Flags.Value)
            |> logDecode device

        let writeValidatedData =
            validateRequest point
            >> bind validatedWrite
     
        e.Message
        |> writeValidatedData
        |> ignore

    let msgSubscribed (e: MqttMsgSubscribedEventArgs) =
        printfn "Sub Message Subscribed: %A" e.GrantedQoSLevels

    let rec mqttConnect (broker : string, port) : MqttClient =
        let client = MqttClient (broker, port, false, null, null, MqttSslProtocols.None)

        try
            client.Connect (Guid.NewGuid().ToString())
            |> function
            | MqttMsgConnack.CONN_REFUSED_PROT_VERS -> failwith "Invalid Protocol Version"
            | MqttMsgConnack.CONN_REFUSED_IDENT_REJECTED -> failwith "Identity Rejected"
            | MqttMsgConnack.CONN_REFUSED_SERVER_UNAVAILABLE -> failwith "Server Unavailable"
            | MqttMsgConnack.CONN_REFUSED_USERNAME_PASSWORD -> failwith "Invalid Username or Password"
            | MqttMsgConnack.CONN_REFUSED_NOT_AUTHORIZED -> failwith "Client Not Authorized"
            | MqttMsgConnack.CONN_ACCEPTED -> client
            | _ -> failwith "Unable to connect: Unknown Connack Type"
        with
            | :? MqttConnectionException ->
                eprintfn "Connection failed. Retrying in 10 seconds."
                Thread.Sleep 10000
                mqttConnect (broker, port)

    // let connectionClosedHandler (client: MqttClient) parameters (e: EventArgs) =
    //     eprintfn "Broker connection closed: trying to reconnect [%A]" e
    //     try
    //         mqttConnect client parameters |> ignore
    //     with
    //     | :? MqttConnectionException ->
    //             Thread.Sleep 10
    //             mqttConnect client parameters |> ignore
