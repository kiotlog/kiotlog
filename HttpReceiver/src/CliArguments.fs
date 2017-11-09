namespace HttpReceiver

open System
open Argu

module CliArguments =
    type Configuration =
        {
            Host: string
            Port: int
            MQTTBroker: string * int
            Postgres: string * int * string * string * string
            Path: string
        }

    let getMqttHost (host, _) = host
    let getMqttPort (_, port) = port

    let getPostgresHost (host, _, _, _, _) = host
    let getPostgresPort (_, port, _, _, _) = port
    let getPostgresUser (_, _, user, _, _) = user
    let getPostgresPass (_, _, _, pass, _) = pass
    let getPostgresDb   (_, _, _, _, db) = db

    type Configuration with
        member c.MqttHost = getMqttHost c.MQTTBroker
        member c.MqttPort = getMqttPort c.MQTTBroker

        member c.PostgresHost = getPostgresHost c.Postgres
        member c.PostgresPort = getPostgresPort c.Postgres
        member c.PostgresUser = getPostgresUser c.Postgres
        member c.PostgresPass = getPostgresPass c.Postgres
        member c.PostgresDb = getPostgresDb c.Postgres

    type HttpReceiverArgs =
        | Host of ip:string
        | Port of port:int
        | MQTTBroker of host:string * port:int
        | Postgres of host:string * port:int * user:string * pass:string * db:string
        | Path of path:string
    with
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | Host _ -> "listening IP address"
                | Port _ -> "listening TCP port"
                | MQTTBroker _ -> "upstream MQTT Broker (host port)"
                | Postgres _ -> "Postgres connectio (host port username password db)"
                | Path _ -> "typed path for callback URL"

    let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)
    let parser = ArgumentParser.Create<HttpReceiverArgs>(programName = "HttpReceiver", errorHandler = errorHandler)

    let parseCLI argv =
        let results = parser.ParseCommandLine argv
        {
            Host = results.GetResult(<@ Host @>, defaultValue = "127.0.0.1")
            Port = results.GetResult(<@ Port @>, defaultValue = 8080)
            MQTTBroker = results.GetResult(<@ MQTTBroker @>, defaultValue = ("127.0.0.1", 1883))
            Postgres = results.GetResult(<@ Postgres @>, defaultValue = ("127.0.0.1", 5432, "postgres", "", "postgres"))
            Path = results.GetResult(<@ Path @>, defaultValue = "/%s/%s/devices/%s/up")
        }

    let config = parser.ParseConfiguration
