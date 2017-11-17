namespace Decoder

open System
open Argu

module Arguments =
    type Configuration =
        {
            Topics: string list
            MQTTBroker: string * int
            Postgres: string * string * string * int *  string
        }

    let getMqttHost (host, _) = host
    let getMqttPort (_, port) = port

    let getPostgresUser (user, _, _, _, _) = user
    let getPostgresPass (_, pass, _, _, _) = pass
    let getPostgresHost (_, _, host, _, _) = host
    let getPostgresPort (_, _, _, port, _) = port
    let getPostgresDb   (_, _, _, _, db) = db
    let getPostgresConnectionString (user, pass, host, port, db) =
        sprintf
            "Username=%s;Password=%s;Host=%s;Port=%d;Database=%s"
            user pass host port db
            

    type Configuration with
        member c.MqttHost = getMqttHost c.MQTTBroker
        member c.MqttPort = getMqttPort c.MQTTBroker

        member c.PostgresHost = getPostgresHost c.Postgres
        member c.PostgresPort = getPostgresPort c.Postgres
        member c.PostgresUser = getPostgresUser c.Postgres
        member c.PostgresPass = getPostgresPass c.Postgres
        member c.PostgresDb = getPostgresDb c.Postgres
        member c.PostgresConnectionString = getPostgresConnectionString c.Postgres

    type HttpReceiverArgs =
        | Topics of topics:string list
        | MQTTBroker of host:string * port:int
        | Postgres of user:string * pass:string * host:string * port:int * db:string
    with
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | Topics _ -> "The list of topics to listen for."
                | MQTTBroker _ -> "upstream MQTT Broker (host port)"
                | Postgres _ -> "Postgres connection (username password host port db)"

    let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)
    let parser = ArgumentParser.Create<HttpReceiverArgs>(programName = "HttpReceiver", errorHandler = errorHandler)

    let parseCLI argv =
        let results = parser.ParseCommandLine argv
        {
            Topics = results.GetResult(<@ Topics @>, defaultValue = [ "+" ])
            MQTTBroker = results.GetResult(<@ MQTTBroker @>, defaultValue = ("127.0.0.1", 1883))
            Postgres = results.GetResult(<@ Postgres @>, defaultValue = ("postgres", "postgres", "127.0.0.1", 5432, "postgres"))
        }

    let config = parser.ParseConfiguration
