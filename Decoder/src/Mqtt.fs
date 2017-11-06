module Trmpln.Mqtt

open System
open System.Threading
open System.Collections.Generic

open uPLibrary.Networking.M2Mqtt
open uPLibrary.Networking.M2Mqtt.Messages
open uPLibrary.Networking.M2Mqtt.Session
open uPLibrary.Networking.M2Mqtt.Internal
open uPLibrary.Networking.M2Mqtt.Exceptions

open Trmpln.Frame
open Trmpln.Struct
open Trmpln.Units
open Trmpln.Helpers

open NGeoHash

open InfluxData.Net.InfluxDb
open InfluxData.Net.Common.Enums
open InfluxData.Net.InfluxDb.Models

// let msgReceivedHandler (i: InfluxDbClient) (e: MqttMsgPublishEventArgs) =
let msgReceivedHandler _ (e: MqttMsgPublishEventArgs) =
    let msg =
        e.Message
        |> decode
        |> TTNFrame.Parse

    let payload =
        msg.PayloadRaw
        |> Convert.FromBase64String
        |> unpack ">BHhH"

    let data = {
        Battery = payload.[1].ToFloat() |> tomVolt |> convertmVToV
        Temperature = payload.[2].ToFloat() |> todCelsius |> convertdCToC
        Pressure = payload.[3].ToFloat() |> todaPascal |> convertdaPaTohPa
    }

    let frame =  {
        Version = payload.[0].ToByte()
        Data = data
        Meta = msg
    }
    printfn "%A" frame

    let lat, lon =
        msg.Metadata.Gateways.[0]
        |> fun gw ->
            gw.Latitude |> float,
            gw.Longitude |> float

    let tags = new Dictionary<string, obj>()
    tags.["AppId"] <- msg.AppId
    tags.["DevId"] <- msg.DevId
    tags.["HardwareSerial"] <- msg.HardwareSerial
    tags.["FrameVersion"] <- payload.[0].ToByte()
    tags.["geohash"] <- GeoHash.Encode(lat, lon, 12)

    let fields = new Dictionary<string, obj>()
    fields.["Battery"] <- convertmVToV << tomVolt <| payload.[1].ToFloat()
    fields.["Temperature"] <- convertdCToC << todCelsius <| payload.[2].ToFloat()
    fields.["Pressure"] <- convertdaPaTohPa << todaPascal  <| payload.[3].ToFloat()

    let point = Point()
    point.Name <- "lopy"
    point.Tags <- tags
    point.Fields <- fields
    point.Timestamp <- Nullable msg.Metadata.Time

    // async {
    //     let! response = i.Client.WriteAsync(point, "ostriot") |> Async.AwaitTask
    //     response |> ignore
    // } |> Async.Start

let msgSubscribed (e: MqttMsgSubscribedEventArgs) =
    printfn "Sub Message Subscribed: %A" e.GrantedQoSLevels

let mqttConnect (client: MqttClient) (clientId, username, password, cleanSession, keepAlivePeriod) =
    client.Connect (clientId, username, password, cleanSession, keepAlivePeriod)
    |> fun connack ->
    match connack with
    | MqttMsgConnack.CONN_REFUSED_PROT_VERS -> failwith "Invalid Protocol Version"
    | MqttMsgConnack.CONN_REFUSED_IDENT_REJECTED -> failwith "Identity Rejected"
    | MqttMsgConnack.CONN_REFUSED_SERVER_UNAVAILABLE -> failwith "Server Unavailable"
    | MqttMsgConnack.CONN_REFUSED_USERNAME_PASSWORD -> failwith "Invalid Username or Password"
    | MqttMsgConnack.CONN_REFUSED_NOT_AUTHORIZED -> failwith "Client Not Authorized"
    | MqttMsgConnack.CONN_ACCEPTED -> connack
    | _ -> failwith "Unknown Connack Type"

let connectionClosedHandler (client: MqttClient) parameters (e: EventArgs) =
    eprintfn "Broker connection closed: trying to reconnect [%A]" e
    try
        mqttConnect client parameters |> ignore
    with
    | :? MqttConnectionException ->
            Thread.Sleep 10
            mqttConnect client parameters |> ignore
