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

open uPLibrary.Networking.M2Mqtt
open uPLibrary.Networking.M2Mqtt.Messages
open uPLibrary.Networking.M2Mqtt.Exceptions

open Chessie.ErrorHandling

open Decoder
open Request

module Mqtt =

    let private validateTopicParts topic c =
        let m = Regex(@"/(\S+)/(\S+)/devices/(\S+)/up").Match(topic)
        if m.Success then
           ok { c with TopicParts = Some (m.Groups.[1].Value, m.Groups.[2].Value, m.Groups.[3].Value) }
        else fail "Unable to parse topic parts."

    let msgReceivedHandler decodeData writeData (e: MqttMsgPublishEventArgs) =

        let ctx = {
            Msg = Some e.Message
            TopicParts = None

            Request = None
            PayloadRaw = None
            
            Datetime = None
            Flags = None
            Data = None
        }

        let validateTopicParts =
            validateTopicParts e.Topic

        let writeValidatedData =
            validateTopicParts
            >> bind validateRequest
            >> bind decodeData
            >> successTee writeData
            >> log "decode"
     
        ctx
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
