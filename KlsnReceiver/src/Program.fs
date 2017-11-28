﻿namespace KlsnReceiver

open System.Threading

open uPLibrary.Networking.M2Mqtt.Messages

open Arguments
open Mqtt
open Klsn
open Writer

module Program =

    [<EntryPoint>]
    let main argv =

        let mainConfig = parseCLI argv

        let mqttTopics, mqttQosLevels =
            mainConfig.Topics |> List.toArray,
            [| for _ in mainConfig.Topics -> MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE |]

        let mqttClient = mqttConnect mainConfig.MQTTBroker

        let mqttPublish =
            mqttPublish mqttClient

        let parseRequest =
            parseRequest mainConfig.PostgresConnectionString

        let writeRequest =
            writePacket mqttPublish

        let msgReceived = msgReceivedHandler parseRequest writeRequest

        mqttClient.MqttMsgPublishReceived.Add msgReceived
        mqttClient.MqttMsgSubscribed.Add msgSubscribed

        mqttClient.Subscribe (mqttTopics, mqttQosLevels) |> ignore

        Thread.Sleep Timeout.Infinite

        0