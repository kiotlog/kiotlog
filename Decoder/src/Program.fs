namespace Decoder

open System.Threading

open uPLibrary.Networking.M2Mqtt.Messages

open Arguments
open Mqtt
open Decoder
open Catalog

module Program =

    [<EntryPoint>]
    let main argv =
        let mainConfig = parseCLI argv

        let decodePayload =
            decodePayload mainConfig.PostgresConnectionString

        let writePoint =
            writePoint mainConfig.PostgresConnectionString

        let mqttTopics, mqttQosLevels =
            mainConfig.Topics |> List.toArray,
            [| for _ in mainConfig.Topics -> MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE |]

        let mqttClient = mqttConnect mainConfig.MQTTBroker

        let msgReceived = msgReceivedHandler decodePayload writePoint

        mqttClient.MqttMsgPublishReceived.Add msgReceived
        mqttClient.MqttMsgSubscribed.Add msgSubscribed

        mqttClient.Subscribe (mqttTopics, mqttQosLevels) |> ignore

        Thread.Sleep Timeout.Infinite

        0
