namespace HttpReceiver

open Suave
open Successful

open MQTT

module WebParts =

    let fromSigFoxPart mqttPath mqttPublisher (origin, devicetypeid, deviceid) : WebPart =
        let path =
            sprintf
                (Printf.StringFormat<string -> string -> string -> string>(mqttPath))
                origin devicetypeid deviceid

        request
            ( fun r  ->
                mqttPublisher (path, r.rawForm)
                |> mqttResultToWebPart OK ACCEPTED )
