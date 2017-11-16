namespace HttpReceiver

open KiotlogDB

module Catalog =

    let getDeviceBasicAuth connectionString devid tkn =

        use ctx = new KiotlogDBContext(connectionString)
    
        let devices =
            query {
                for d in ctx.Devices do  
                where (d.Device = devid)
                select (d.Device, d.Auth)
            } |> Seq.toArray
        
        let checkBasicAuth (device, auth: Devices.JsonBAuth) =
            device = devid && auth.Basic.Token = tkn
        
        devices |> Seq.tryFind checkBasicAuth
