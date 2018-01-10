(*
    Copyright (C) 2017 Giampaolo Mancini, Trampoline SRL.
    Copyright (C) 2017 Francesco Varano, Trampoline SRL.

    This file is part of Kiotlog.

    Kiotlog is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Kiotlog is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*)

namespace HttpReceiver

open System
open System.Threading

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
    let rec mqttConnect (broker : string, port) : MqttClient =
        let client = MqttClient (broker, port, false, null, null, MqttSslProtocols.None)
        let clientId = "HttpReceiver/" + Guid.NewGuid().ToString()

        try
            match client.Connect clientId with
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
