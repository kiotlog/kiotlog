namespace HttpReceiver

open KiotlogDB
open Microsoft.FSharpLu.Json

module Authorization =

    type BasicAuth =
        {
            Token: string
        }

    type Auth =
        {
            Basic: BasicAuth
        }

module Catalog =

    open Authorization

    let getDeviceBasicAuth (ctx: trmplnContext) devid tkn =
    
        let devices =
            query {
                for d in ctx.Devices do  
                where (d.Device = devid)
                select (d.Device, d.Auth)
            } |> Seq.toArray
        
        let checkBasicAuth (device, auth) =
            let auth : Auth = Default.deserialize auth
            device = devid && auth.Basic.Token = tkn
        
        devices |> Seq.tryFind checkBasicAuth
