namespace Decoder

// open uPLibrary.Networking.M2Mqtt.Session
// open uPLibrary.Networking.M2Mqtt.Internal
// open uPLibrary.Networking.M2Mqtt.Exceptions
// open System.Threading
// open NGeoHash
// open Units

open System
open Microsoft.FSharpLu.Json
open System.Text.RegularExpressions

open uPLibrary.Networking.M2Mqtt
open uPLibrary.Networking.M2Mqtt.Messages

open Decoder
open Body
open Json
open Helpers

module Mqtt =

    let msgReceivedHandler (cs: string) (e: MqttMsgPublishEventArgs) =

        // Decode Message and Deserialize JSON
        let msg : KlBody = 
            e.Message
            |> decode
            |> SnakeCaseSerializer.deserialize
        
        let channel, app, device =
            let m = Regex(@"/(\S+)/(\S+)/devices/(\S+)/up").Match(e.Topic)
            m.Groups.[1].Value, m.Groups.[2].Value, m.Groups.[3].Value

        klDecode cs (channel, app, device) msg

    let msgSubscribed (e: MqttMsgSubscribedEventArgs) =
        printfn "Sub Message Subscribed: %A" e.GrantedQoSLevels

    let mqttConnect (broker : string, port) =
        let client = MqttClient(broker, port, false, null, null, MqttSslProtocols.None)

        client.Connect(Guid.NewGuid().ToString())
        |> function
        | MqttMsgConnack.CONN_REFUSED_PROT_VERS -> failwith "Invalid Protocol Version"
        | MqttMsgConnack.CONN_REFUSED_IDENT_REJECTED -> failwith "Identity Rejected"
        | MqttMsgConnack.CONN_REFUSED_SERVER_UNAVAILABLE -> failwith "Server Unavailable"
        | MqttMsgConnack.CONN_REFUSED_USERNAME_PASSWORD -> failwith "Invalid Username or Password"
        | MqttMsgConnack.CONN_REFUSED_NOT_AUTHORIZED -> failwith "Client Not Authorized"
        | MqttMsgConnack.CONN_ACCEPTED -> client
        | _ -> failwith "Unable to connect: Unknown Connack Type"

    // let connectionClosedHandler (client: MqttClient) parameters (e: EventArgs) =
    //     eprintfn "Broker connection closed: trying to reconnect [%A]" e
    //     try
    //         mqttConnect client parameters |> ignore
    //     with
    //     | :? MqttConnectionException ->
    //             Thread.Sleep 10
    //             mqttConnect client parameters |> ignore
