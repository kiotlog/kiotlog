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

    let private tryConnect (client: MqttClient) clientId =
        try
            match client.Connect clientId with
            | MqttMsgConnack.CONN_REFUSED_PROT_VERS -> fail "Invalid Protocol Version"
            | MqttMsgConnack.CONN_REFUSED_IDENT_REJECTED -> fail "Identity Rejected"
            | MqttMsgConnack.CONN_REFUSED_SERVER_UNAVAILABLE -> fail "Server Unavailable"
            | MqttMsgConnack.CONN_REFUSED_USERNAME_PASSWORD -> fail "Invalid Username or Password"
            | MqttMsgConnack.CONN_REFUSED_NOT_AUTHORIZED -> fail "Client Not Authorized"
            | MqttMsgConnack.CONN_ACCEPTED -> ok client
            | _ -> fail "Unable to connect: Unknown Connack Type"
        with
            | :? MqttConnectionException as ex ->
                fail ex.Message

    let rec mqttConnect (broker : string, port) clientId =
        let client = MqttClient (broker, port, false, null, null, MqttSslProtocols.None)

        match tryConnect client clientId with
        | Ok (client, _) ->
            printfn "Connected."
            client
        | Bad msg ->
            eprintfn "Connection failed. Retrying in 1 second. [%A]" msg
            Thread.Sleep 1000
            mqttConnect (broker, port) clientId

    let rec mqttClosed client clientId e =
        eprintfn "Broker closed connection: trying to reconnect."
        match tryConnect client clientId with
        | Ok _ ->
            printfn "Connected."
        | Bad msg ->
            eprintfn "Re-opening closed connection failed. Retrying in 1 second. [%A]" msg
            Thread.Sleep 1000
            mqttClosed client clientId e
