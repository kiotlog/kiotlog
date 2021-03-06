﻿(*
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

module HttpReceiver.Program

open Microsoft.EntityFrameworkCore
open KiotlogDBF.Context

open Suave
open Suave.Operators
open Suave.Filters

open HttpReceiver.Catalog
open HttpReceiver.HttpAuth
open HttpReceiver.WebParts
open HttpReceiver.MQTT
open HttpReceiver.CliArguments
open HttpReceiver.Helpers

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

    let dbContextOptions =
        DbContextOptionsBuilder<KiotlogDBFContext>()
            .UseNpgsql(mainConfig.PostgresConnectionString)
            .Options

    let getDeviceBasicAuth =
        getDeviceBasicAuth dbContextOptions

    let checkToken =
        checkToken getDeviceBasicAuth

    let app =
        Authentication.authenticateBasic
            checkToken
            routes

    startWebServer
        httpConfig
        app
    0
