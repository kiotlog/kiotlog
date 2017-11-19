namespace HttpReceiver

open System

open Chessie.ErrorHandling

open uPLibrary.Networking.M2Mqtt
open uPLibrary.Networking.M2Mqtt.Messages
open uPLibrary.Networking.M2Mqtt.Exceptions

module MQTT =

    /// Convert two track Result<'TSuccess, 'TMessage> input to track output type via provided functions.
    let mqttResultToWebPart success failure =
        function
        | Ok (err, msgs) -> err.ToString()::msgs |> String.concat " - " |> success
        | Bad errors -> errors |> String.concat " - " |> failure

    /// Publish data to given channel via MQTT client.
    /// Wrap MqttClient.Publish() call in Result type.
    let mqttPublish (client : MqttClient) topic data =
        try
            client.Publish (topic, data) |> ok
        with
        | :? MqttClientException as ex -> fail ex.Message

    /// Connect to MQTT broker
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
