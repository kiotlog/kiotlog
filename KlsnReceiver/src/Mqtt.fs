namespace KlsnReceiver

open System
open System.Threading
open System.Text.RegularExpressions

open Chessie.ErrorHandling
open Klsn

module Mqtt =

    open uPLibrary.Networking.M2Mqtt
    open uPLibrary.Networking.M2Mqtt.Messages
    open uPLibrary.Networking.M2Mqtt.Exceptions

    let private validateTopicParts topic req =
        let m = Regex(@"/(\S+)/(\S+)/(\S+)").Match(topic)
        if m.Success then
           ok { req with TopicParts = Some (m.Groups.[1].Value, m.Groups.[2].Value, m.Groups.[3].Value) }
        else fail "Unable to parse topic parts."

    let msgReceivedHandler parseRequest writeData (e: MqttMsgPublishEventArgs) =

        let now = DateTime.UtcNow

        let req =
            {
                TopicParts = None
                Msg = Some e.Message
                Device = None
                Key = None
                Packet = None
                Time = Some now
                Payload = None
            }

        let validateTopicParts =
            validateTopicParts e.Topic

        let usecase =
            validateTopicParts
            >> bind parseRequest
            >> successTee writeData
            >> log "request"

        req
        |> usecase
        |> ignore

    let msgSubscribed (e: MqttMsgSubscribedEventArgs) =
        printfn "Sub Message Subscribed: %A" e.GrantedQoSLevels

    let mqttPublish (client : MqttClient) topic data =
        try
            client.Publish (topic, data) |> ok
        with
        | :? MqttClientException as ex -> fail ex.Message

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
