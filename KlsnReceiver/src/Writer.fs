namespace KlsnReceiver

open System
open System.Text
open Newtonsoft.Json
open Newtonsoft.Json.Serialization

// open Chessie.ErrorHandling

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
