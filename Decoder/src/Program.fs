module Decoder.Program

open System.Threading

// open uPLibrary.Networking.M2Mqtt
open uPLibrary.Networking.M2Mqtt.Messages

open Arguments
open Mqtt
open Catalog

[<EntryPoint>]
let main argv =
    let mainConfig = parseCLI argv

    let decodePayload =
        klDecode mainConfig.PostgresConnectionString

    let writePayload =
        klWrite mainConfig.PostgresConnectionString

    let mqttTopics, mqttQosLevels =
        mainConfig.Topics |> List.toArray,
        [| for _ in mainConfig.Topics -> MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE |]

    let mqttClient = mqttConnect mainConfig.MQTTBroker

    let msgReceived = msgReceivedHandler decodePayload writePayload

    mqttClient.MqttMsgPublishReceived.Add msgReceived
    mqttClient.MqttMsgSubscribed.Add msgSubscribed
    
    mqttClient.Subscribe (mqttTopics, mqttQosLevels) |> ignore

    Thread.Sleep Timeout.Infinite

    0

