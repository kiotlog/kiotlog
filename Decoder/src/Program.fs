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

namespace Decoder

open System.Threading

open Microsoft.EntityFrameworkCore
open KiotlogDBF.Context

open uPLibrary.Networking.M2Mqtt.Messages

open Decoder.Arguments
open Decoder.Mqtt
open Decoder.Decoder
open Decoder.Catalog

module Program =

    [<EntryPoint>]
    let main argv =
        let mainConfig = parseCLI argv

        let options =
            DbContextOptionsBuilder<KiotlogDBFContext>()
                .UseNpgsql(mainConfig.PostgresConnectionString, fun options ->
                    options.EnableRetryOnFailure() |> ignore)
                .Options

        let decodePayload =
            decodePayload options

        let writePoint =
            writePoint options

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
