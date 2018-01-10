(*
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
        let decode (rawForm : byte []) =
            Encoding.UTF8.GetString rawForm

        req.rawForm |> decode |> fromJson<'a>

module Helpers =
    type System.String with
        member s.ToPF () = PrintfFormat<_,_,_,_,_>(s)
