namespace HttpReceiver

open FSharp.Data.Sql
open FSharp.Data

module Catalog =

    let [<Literal>] DeviceAuthSample = """ { "basic": {"token": "friendlytoken"}} """
    type DeviceAuth = JsonProvider<DeviceAuthSample>

    // let runtimeConnString = @"User ID=kl_httprecv;Host=postgres;Port=5432;Database=trmpln;Password=KlHttpr3cv"
    // type Sql = SqlDataProvider<DbVendor, ConnString, ResolutionPath = ResolutionPath, IndivAmount, UseOptTypes>
    type Sql =
        SqlDataProvider<
            DatabaseVendor = Common.DatabaseProviderTypes.POSTGRESQL,
            ConnectionString = @"User ID=kl_httprecv;Host=localhost;Port=7432;Database=trmpln;Password=KlHttpr3cv",
            ResolutionPath = @"dlls",
            IndividualsAmount = 1000,
            UseOptionTypes = true,
            Owner = @"public">


    let getDeviceBasicAuth (host, port, user, pass, db) devid tkn =
    
        let runtimeConnString =
            sprintf "Host=%s;Port=%d;User ID=%s;Password=%s;Database=%s" host port user pass db
        
        printfn "Connecting to: %s" runtimeConnString

        let ctx = Sql.GetDataContext(connectionString = runtimeConnString, resolutionPath = ".")
    
        let devices =
            query {
                for d in ctx.Public.Devices do  
                where (d.Device = devid)
                select (d.Device, DeviceAuth.Parse d.Auth)
            } |> Seq.toArray
        
        let checkBasicAuth (device, auth: JsonProvider<DeviceAuthSample>.Root) =
            device = devid && auth.Basic.Token = tkn
        
        devices |> Seq.tryFind checkBasicAuth
