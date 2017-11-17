module Decoder.Program

open System.Threading

// open uPLibrary.Networking.M2Mqtt
open uPLibrary.Networking.M2Mqtt.Messages

open Arguments
open Mqtt

[<EntryPoint>]
let main argv =
    let mainConfig = parseCLI argv

    let mqttTopics, mqttQosLevels =
        mainConfig.Topics |> List.toArray,
        [| for _ in mainConfig.Topics -> MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE |]

    let mqttClient = mqttConnect mainConfig.MQTTBroker

    let msgReceived = msgReceivedHandler mainConfig.PostgresConnectionString

    mqttClient.MqttMsgPublishReceived.Add msgReceived
    mqttClient.MqttMsgSubscribed.Add msgSubscribed
    
    mqttClient.Subscribe (mqttTopics, mqttQosLevels) |> ignore

    Thread.Sleep Timeout.Infinite

    0

