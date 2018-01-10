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
