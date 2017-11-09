namespace HttpReceiver

module HttpAuth =

    let checkToken f (devid: string, tkn: string) =
        match f devid tkn with
        | Some _ -> true
        | None -> false