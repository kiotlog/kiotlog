namespace HttpReceiver

open Microsoft.EntityFrameworkCore
open KiotlogDB

module Catalog =

    let getDeviceBasicAuth (cs : string) devid tkn =

        let optionsBuilder = DbContextOptionsBuilder<KiotlogDBContext>()
        optionsBuilder.UseNpgsql(cs) |> ignore

        use dbCtx = new KiotlogDBContext(optionsBuilder.Options)

        let devices =
            query {
                for d in dbCtx.Devices do
                where (d.Device = devid)
                select (d.Device, d.Auth)
            } |> Seq.toArray

        let checkBasicAuth (device, auth: Devices.JsonBAuth) =
            device = devid && auth.Basic.Token = tkn

        devices |> Seq.tryFind checkBasicAuth
