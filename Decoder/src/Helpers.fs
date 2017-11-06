module Trmpln.Helpers

open System.Text

let decode (data: byte[]) =
    data |> Encoding.ASCII.GetString

let encode (str: string) =
    str |> Encoding.ASCII.GetBytes