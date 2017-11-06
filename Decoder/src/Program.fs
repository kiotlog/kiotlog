module Trmpln.OstriotCli

open System
open System.Threading

open FSharp.Data

open uPLibrary.Networking.M2Mqtt
open uPLibrary.Networking.M2Mqtt.Messages

open InfluxData.Net.InfluxDb
open InfluxData.Net.Common.Enums

open Trmpln.Arguments
open Trmpln.Struct
open Trmpln.Frame
open Trmpln.Units
open Trmpln.Mqtt

[<EntryPoint>]
let main argv =
    let mc = parseCLI argv

    let influxclient =
        InfluxDbClient
            (mc.HostInfluxDB, mc.UserInfluxDB, mc.PassInfluxDB, InfluxDbVersion.v_1_0_0)

    let ttnClient, ttnConnectionParams, ttnTopics, ttnQosLevels =
        MqttClient (brokerHostName = mc.BrokerMQTT), 
        ("ostriotcli/" + Guid.NewGuid().ToString(), mc.UserMQTT, mc.PassMQTT, false, 60us), 
        [| for ln in mc.LoraNodes -> "+/devices/" + ln + "/up" |], 
        [| for _ in mc.LoraNodes -> MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE |]

    let msgReceived = msgReceivedHandler influxclient
    let connectionClosed = connectionClosedHandler ttnClient ttnConnectionParams

    ttnClient.MqttMsgPublishReceived.Add msgReceived
    ttnClient.MqttMsgSubscribed.Add msgSubscribed
    ttnClient.ConnectionClosed.Add connectionClosed

    mqttConnect ttnClient ttnConnectionParams |> ignore
    ttnClient.Subscribe (ttnTopics, ttnQosLevels) |> ignore

    Thread.Sleep Timeout.Infinite

    0

