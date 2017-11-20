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
open Json
open Helpers
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open System.Collections.Generic

module Mqtt =

    let msgReceivedHandler decodeData writeData (e: MqttMsgPublishEventArgs) =

        let msg = 
            e.Message
            |> decode
            |> JObject.Parse
        
        let channel, app, device =
            let m = Regex(@"/(\S+)/(\S+)/devices/(\S+)/up").Match(e.Topic)
            m.Groups.[1].Value, m.Groups.[2].Value, m.Groups.[3].Value

        let decodedMeta =
            msg.["metadata"].ToString(Formatting.None)
        
        let decodedTime =
            let time = msg.["metadata"].["time"]
            match channel with
            | "sigfox" -> time |> int64 |> unixTimeStampToDateTime
            | "lorawan" -> time |> string |> DateTime.Parse
            | _ -> DateTime.Now                     

        let decodedDict =
            let devId = string msg.["dev_id"]
            let payloadRaw = string msg.["payload_raw"]
            let data : Dictionary<string, float> option =
                decodeData (channel, app, device) devId payloadRaw
            
            match data with
            | None -> None
            | Some data -> data |> SnakeCaseSerializer.serialize |> Some


        printfn "[%A] [%s] %O" decodedTime device decodedDict
        printfn "[%A] [%s] %A" decodedTime device decodedMeta
        writeData (device, decodedTime, decodedMeta, decodedDict)

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
