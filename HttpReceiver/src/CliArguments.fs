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

namespace HttpReceiver

open System
open Argu

module CliArguments =
    type Configuration =
        {
            Host: string
            Port: int
            MQTTBroker: string * int
            Postgres: string * string * string * int *  string
            Path: string
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
        | Host of ip:string
        | Port of port:int
        | MQTTBroker of host:string * port:int
        | Postgres of user:string * pass:string * host:string * port:int * db:string
        | Path of path:string
    with
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | Host _ -> "listening IP address"
                | Port _ -> "listening TCP port"
                | MQTTBroker _ -> "upstream MQTT Broker (host port)"
                | Postgres _ -> "Postgres connection (username password host port db)"
                | Path _ -> "typed path for callback URL"

    let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)
    let parser = ArgumentParser.Create<HttpReceiverArgs>(programName = "HttpReceiver", errorHandler = errorHandler)

    let parseCLI argv =
        let results = parser.ParseCommandLine argv
        {
            Host = results.GetResult(<@ Host @>, defaultValue = "127.0.0.1")
            Port = results.GetResult(<@ Port @>, defaultValue = 8080)
            MQTTBroker = results.GetResult(<@ MQTTBroker @>, defaultValue = ("127.0.0.1", 1883))
            Postgres = results.GetResult(<@ Postgres @>, defaultValue = ("postgres", "postgres", "127.0.0.1", 5432, "postgres"))
            Path = results.GetResult(<@ Path @>, defaultValue = "/%s/%s/devices/%s/up")
        }

    let config = parser.ParseConfiguration
