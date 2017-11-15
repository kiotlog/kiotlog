namespace HttpReceiver

open Microsoft.FSharpLu.Json

open KiotlogDB


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

    let getDeviceBasicAuth connectionString devid tkn =

        use ctx = new KiotlogDBContext(connectionString)
    
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
