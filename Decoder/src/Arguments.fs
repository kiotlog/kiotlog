namespace Trmpln

open System
open Argu

module Arguments =
    type OstriotConf =
        {
            LoraNodes: string List
            UserMQTT: string
            PassMQTT: string
            BrokerMQTT: string
            HostInfluxDB: string
            UserInfluxDB: string
            PassInfluxDB: string
        }

    type OstriotCliArguments =
        | [<MainCommand; Last>] Nodes of nodes:string List
        | Username of username:string
        | Password of password:string
        | Broker of url:string
        | InfluxHost of url:string
        | InfluxUser of username:string
        | InfluxPass of password:string
    with
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | Nodes _ -> "Specify a list of LoRa nodes."
                | Username _ -> "Username for MQTT broker or The Things Network."
                | Password _ -> "Password for MQTT borker or The Things Network."
                | Broker _ -> "URL of MQTT broker."
                | InfluxHost _ -> "URL of InfluxDB host"
                | InfluxUser _ -> "Username for InfluxDB DB"
                | InfluxPass _ -> "Password for InfluxDB DB"

    let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)
    let parser = ArgumentParser.Create<OstriotCliArguments>(programName = "ostriot-cli", errorHandler = errorHandler)

    let parseCLI argv =
        let r = parser.ParseCommandLine argv
        {
            LoraNodes = r.GetResult(<@ Nodes @>, defaultValue = [ "+" ])
            UserMQTT = r.GetResult(<@ Username @>, defaultValue = "trmpln-test")
            PassMQTT = r.GetResult(<@ Password @>, defaultValue = "ttn-account-v2.Q9XhSxNi79LX8cFx0UKytSCItXzJy2DtW5lhJcI49MI")
            BrokerMQTT = r.GetResult(<@ Broker @>, defaultValue = "eu.thethings.network")
            HostInfluxDB = r.GetResult(<@ InfluxHost @>, defaultValue = "http://localhost:8086/")
            UserInfluxDB = r.GetResult(<@ InfluxUser @>, defaultValue = "ostriotcli")
            PassInfluxDB = r.GetResult(<@ InfluxPass @>, defaultValue = "OsTrIoTPasswd")
        }