namespace HttpReceiver

open System
open Argu

module CliArguments =
    type Configuration =
        {
            Host: string
            Port: int
            MQTTBroker: string * int
            Path: string
        }

    let getMqttHost (host, _) = host
    let getMqttPort (_, port) = port

    type Configuration with
        member c.MqttHost = getMqttHost c.MQTTBroker
        member c.MqttPort = getMqttPort c.MQTTBroker

    type HttpReceiverArgs =
        | Host of ip:string
        | Port of port:int
        | MQTTBroker of host:string * port:int
        | Path of path:string
    with
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | Host _ -> "listening IP address"
                | Port _ -> "listening TCP port"
                | MQTTBroker _ -> "upstream MQTT Broker (host : port)"
                | Path _ -> "typed path for callback URL"

    let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)
    let parser = ArgumentParser.Create<HttpReceiverArgs>(programName = "HttpReceiver", errorHandler = errorHandler)

    let parseCLI argv =
        let results = parser.ParseCommandLine argv
        {
            Host = results.GetResult(<@ Host @>, defaultValue = "127.0.0.1")
            Port = results.GetResult(<@ Port @>, defaultValue = 8080)
            MQTTBroker = results.GetResult(<@ MQTTBroker @>, defaultValue = ("127.0.0.1", 1883))
            Path = results.GetResult(<@ Path @>, defaultValue = "/%s/%s/devices/%s/up")
        }

    let config = parser.ParseConfiguration
