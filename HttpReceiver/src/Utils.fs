namespace HttpReceiver

open System.Text

open Newtonsoft.Json
open Newtonsoft.Json.Serialization

open Suave
open Operators
open Successful

module Json =

    let JSON v =
        let settings = JsonSerializerSettings()
        settings.ContractResolver <- CamelCasePropertyNamesContractResolver()

        JsonConvert.SerializeObject(v, settings)
        |> OK
        >=> Writers.setMimeType "application/json; charset=utf-8"

    let fromJson<'a> json =
        JsonConvert.DeserializeObject<'a> json

    let getResourceFromReq<'a> (req : HttpRequest) =
        let decode rawForm =
            Encoding.UTF8.GetString rawForm

        req.rawForm |> decode |> fromJson<'a>

module Helpers =
    type System.String with
        member s.ToPF () = PrintfFormat<_,_,_,_,_>(s)
