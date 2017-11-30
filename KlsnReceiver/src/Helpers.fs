namespace KlsnReceiver

open System.Text
open System

module Helpers =

    let decode (data: byte[]) =
        data |> Encoding.ASCII.GetString

    let encode (str: string) =
        str |> Encoding.ASCII.GetBytes

    let byteArrayFromHexString (hex: string) =
        let joinTwoChars = Seq.map (string) >> Seq.reduce (+)
        let hexCharsToByte chs = Convert.ToByte (chs, 16)
        let twoCharsToByte = joinTwoChars >> hexCharsToByte

        hex
        |> Seq.chunkBySize 2
        |> Seq.map twoCharsToByte
        |> Seq.toArray

    let hexStringFromByteArray (bytes : byte []) =
        bytes
        |> Array.map (fun b -> sprintf "%02x" b)
        |> Array.reduce (+)

    let unixTimeStampToDateTime ts =
         let dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds ts
         dateTimeOffset.UtcDateTime
         