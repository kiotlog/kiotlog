module HttpReceiver.Program

open Suave
open Operators
open Filters

open Catalog
open HttpAuth
open WebParts
open MQTT
open CliArguments
open Helpers
open KiotlogDB

[<EntryPoint>]
let main argv =
    let mainConfig = parseCLI argv

    let mqttPublish =
        mainConfig.MQTTBroker
        |> mqttConnect
        |> mqttPublish

    let fromSigFoxPart =
        fromSigFoxPart
            mainConfig.Path
            mqttPublish

    let httpConfig =
        { defaultConfig with
            bindings =
                [ HttpBinding.createSimple
                    HTTP
                    mainConfig.Host
                    mainConfig.Port ]
        }

    let routes =
        choose [
            POST >=> choose [
                pathScan
                    (mainConfig.Path.ToPF())
                    fromSigFoxPart
            ]
        ]
    
    let checkToken =
        checkToken (getDeviceBasicAuth mainConfig.PostgresConnectionString)

    let app =
        Authentication.authenticateBasic
            checkToken
            routes

    startWebServer
        httpConfig
        app
    0
