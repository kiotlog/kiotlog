namespace HttpReceiver

module Catalog =

    type Device =
        {
            ID: string
            Token: string
        }

    let DeviceCatalog = [
            { ID = "18B8D6"; Token = "fb2e2658-c6cd-498d-91e6-37ad92bbe89b"}
            { ID = "18A9E5"; Token = "afa1d877-f4f7-4b38-b559-48fd00d9ae1b"}
        ]

    let getDevice devid tkn =
            DeviceCatalog
            |> List.tryFind ( fun x -> x.ID = devid && x.Token = tkn )

    let checkToken (devid, tkn) =
        match getDevice devid tkn with
        | Some _ -> true
        | None -> false
