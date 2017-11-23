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
open Catalog
open Request

module Mqtt =

    let msgReceivedHandler decodeData writeData (e: MqttMsgPublishEventArgs) =

        let topicParts =
            let m = Regex(@"/(\S+)/(\S+)/devices/(\S+)/up").Match(e.Topic)
            if m.Success
                then m.Groups.[1].Value, m.Groups.[2].Value, m.Groups.[3].Value
                else ("", "", "")

        let ctx = {
            TopicParts = topicParts
            Request = None
            PayloadRaw = None
            
            Datetime = None
            Flags = None
            Data = None
        }

        let writeValidatedData =    
            validateRequest ctx
            >> bind (validatedWrite decodeData writeData)
     
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
