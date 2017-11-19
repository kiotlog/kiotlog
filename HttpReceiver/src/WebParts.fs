namespace HttpReceiver

open Suave
open Successful

open MQTT

module WebParts =

    let fromSigFoxPart mqttPath mqttPublisher (channel, application, deviceid) : WebPart =
        let path =
            sprintf
                (Printf.StringFormat<string -> string -> string -> string>(mqttPath))
                channel application deviceid

        request
            ( fun r  ->
                mqttPublisher path r.rawForm
                |> mqttResultToWebPart OK ACCEPTED )
