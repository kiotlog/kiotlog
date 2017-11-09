namespace HttpReceiver

open FSharp.Data.Sql
open FSharp.Data

module Catalog =

    let [<Literal>] DeviceAuthSample = """ { "basic": {"token": "friendlytoken"}} """
    type DeviceAuth = JsonProvider<DeviceAuthSample>

    type Sql =
        SqlDataProvider<
            DatabaseVendor = Common.DatabaseProviderTypes.POSTGRESQL,
            ConnectionString = @"User ID=kl_grafana;Password=KlGr4f4n4;Host=localhost;Port=7432;Database=trmpln",
            ResolutionPath = @"dlls",
            IndividualsAmount = 1000,
            UseOptionTypes = true,
            Owner = @"public">


    let getDeviceBasicAuth (host, port, user, pass, db) devid tkn =
    
        let runtimeConnString =
            sprintf "Host=%s;Port=%d;User ID=%s;Password=%s;Database=%s" host port user pass db
        
        let ctx = Sql.GetDataContext runtimeConnString
    
        let devices =
            query {
                for d in ctx.Public.Devices do  
                where (d.Device = devid)
                select (d.Device, DeviceAuth.Parse d.Auth)
            } |> Seq.toArray
        
        let checkBasicAuth (device, auth: JsonProvider<DeviceAuthSample>.Root) =
            device = devid && auth.Basic.Token = tkn
        
        devices |> Seq.tryFind checkBasicAuth
