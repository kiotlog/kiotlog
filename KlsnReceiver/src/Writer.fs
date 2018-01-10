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

namespace KlsnReceiver

open System
open System.Text
open Newtonsoft.Json
open Newtonsoft.Json.Serialization

open Klsn

module Writer =

    type Meta =
        {
            Time : int64
        }

    type DsPacket =
        {
            DevId : string
            PayloadRaw : string
            Metadata : Meta
        }

    let private buildPacket (req : KlsnRequest) =
        let dto = DateTimeOffset(req.Time.Value)
        {
            DevId = req.Device.Value.Device
            PayloadRaw = req.Payload.Value |> Convert.ToBase64String
            Metadata = { Time = dto.ToUnixTimeSeconds() }
        }

    let private dumpDsPacket (pkt : DsPacket) =
        let snakeSettings =
            JsonSerializerSettings(
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Error,
                ContractResolver = DefaultContractResolver (
                    NamingStrategy = SnakeCaseNamingStrategy(true, false, false)
                )
            )
        JsonConvert.SerializeObject(pkt, snakeSettings)
        |> Encoding.ASCII.GetBytes

    let writePacket publisher (req : KlsnRequest, _) =
        let channel, app, device = req.TopicParts.Value
        let pubTopic = sprintf "/%s/%s/devices/%s/up" channel app device

        buildPacket req
        |> dumpDsPacket
        |> publisher pubTopic
        |> ignore
