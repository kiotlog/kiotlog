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

namespace Decoder

open System
open System.IO

open Chessie.ErrorHandling

module PackedValue =

    type PackedValue =
        | S8  of sbyte
        | U8  of byte
        | S16 of int16
        | U16 of uint16
        | S32 of int32
        | U32 of uint32
        | S64 of int64
        | U64 of uint64
        | F32 of single
        | F64 of double

    let toSByte x =
        match x with
        | S8 value -> value
        | _ -> failwith "Value of incompatible type"

    let toByte x =
        match x with
        | U8 value -> value
        | _ -> failwith "Value of incompatible type"

    let toInt16 x =
        match x with
        | S16 value -> value
        | S8 value  -> int16 value
        | U8 value  -> int16 value
        | _ -> failwith "Value of incompatible type"

    let toUInt16 x =
        match x with
        | U16 value -> value
        | S8 value  -> uint16 value
        | U8 value  -> uint16 value
        | _ -> failwith "Value of incompatible type"

    let toInt32 x =
        match x with
        | S32 value -> value
        | S16 value -> int32 value
        | U16 value -> int32 value
        | S8 value  -> int32 value
        | U8 value  -> int32 value
        | _ -> failwith "Value of incompatible type"

    let toUInt32 x =
        match x with
        | U32 value -> value
        | S16 value -> uint32 value
        | U16 value -> uint32 value
        | S8 value  -> uint32 value
        | U8 value  -> uint32 value
        | _ -> failwith "Value of incompatible type"

    let toInt64 x =
        match x with
        | S64 value -> value
        | S32 value -> int64 value
        | U32 value -> int64 value
        | S16 value -> int64 value
        | U16 value -> int64 value
        | S8 value  -> int64 value
        | U8 value  -> int64 value
        | _ -> failwith "Value of incompatible type"

    let toUInt64 x =
        match x with
        | U64 value -> value
        | S32 value -> uint64 value
        | U32 value -> uint64 value
        | S16 value -> uint64 value
        | U16 value -> uint64 value
        | S8 value  -> uint64 value
        | U8 value  -> uint64 value
        | _ -> failwith "Value of incompatible type"

    let toSingle x =
        match x with
        | F32 value -> value
        | S64 value -> single value
        | U64 value -> single value
        | S32 value -> single value
        | U32 value -> single value
        | S16 value -> single value
        | U16 value -> single value
        | S8 value  -> single value
        | U8 value  -> single value
        | _ -> failwith "Value of incompatible type"

    let toDouble x =
        match x with
        | F64 value -> value
        | F32 value -> float value
        | S64 value -> float value
        | U64 value -> float value
        | S32 value -> float value
        | U32 value -> float value
        | S16 value -> float value
        | U16 value -> float value
        | S8 value  -> float value
        | U8 value  -> float value

    let adjustEndianNess x =
        match x with
        | S8 _ | U8 _ -> x
(*      | S16 value -> value |>          IPAddress.NetworkToHostOrder           |> S16
        | U16 value -> value |> int16 |> IPAddress.NetworkToHostOrder |> uint16 |> U16
        | S32 value -> value |>          IPAddress.NetworkToHostOrder           |> S32
        | U32 value -> value |> int32 |> IPAddress.NetworkToHostOrder |> uint32 |> U32
        | S64 value -> value |>          IPAddress.NetworkToHostOrder           |> S64
        | U64 value -> value |> int64 |> IPAddress.NetworkToHostOrder |> uint64 |> U64 *)
        | S16 value -> value |> BitConverter.GetBytes |> Array.rev |> fun bytes -> BitConverter.ToInt16  (bytes, 0) |> S16
        | U16 value -> value |> BitConverter.GetBytes |> Array.rev |> fun bytes -> BitConverter.ToUInt16 (bytes, 0) |> U16
        | S32 value -> value |> BitConverter.GetBytes |> Array.rev |> fun bytes -> BitConverter.ToInt32  (bytes, 0) |> S32
        | U32 value -> value |> BitConverter.GetBytes |> Array.rev |> fun bytes -> BitConverter.ToUInt32 (bytes, 0) |> U32
        | S64 value -> value |> BitConverter.GetBytes |> Array.rev |> fun bytes -> BitConverter.ToInt64  (bytes, 0) |> S64
        | U64 value -> value |> BitConverter.GetBytes |> Array.rev |> fun bytes -> BitConverter.ToUInt64 (bytes, 0) |> U64
        | F32 value -> value |> BitConverter.GetBytes |> Array.rev |> fun bytes -> BitConverter.ToSingle (bytes, 0) |> F32
        | F64 value -> value |> BitConverter.GetBytes |> Array.rev |> fun bytes -> BitConverter.ToDouble (bytes, 0) |> F64

    type PackedValue with
        member x.ToSByte () = toSByte x
        member x.ToByte () = toByte x
        member x.ToInt16 () = toInt16 x
        member x.ToUInt16 () = toUInt16 x
        member x.ToInt32 () = toInt32 x
        member x.ToUInt32 () = toUInt32 x
        member x.ToInt64 () = toInt64 x
        member x.ToUInt64 () = toUInt64 x
        member x.ToSingle () = toSingle x
        member x.ToDouble () = toDouble x
        member x.AdjustEndianNess () = adjustEndianNess x
        // https://stackoverflow.com/questions/5312182/conversion-between-types-in-discriminated-unions

module Struct =

    open PackedValue

    type Endiannes =
        | Native
        | Little
        | Big
        | Network

    type Repetition = Repetition of int

    type FormatChar =
        | Endiannes of Endiannes
        | Repetition of Repetition
        | Value of PackedValue

    let private explodeFormatString =
        // TODO: Check for empty format string

        let (|Digit|_|) (ch : char) =
           match ch |> string |> System.Int32.TryParse with
           | true, value -> Some(value)
           | _ -> None

        let (|Character|_|) (ch: char) =
            match ch with
            | '@' | '=' | '<' | '>' | '!'
            | 'b' | 'B'
            | 'h' | 'H'
            | 'i' | 'I' | 'l' | 'L'
            | 'f' | 'd'
            | 'q' | 'Q' -> Some ch
            | _ -> None

        let parseFormatAtom =
            let repetitions : int option ref = ref None
            function
            | Digit d ->
                match !repetitions with
                | None -> // first digit
                    repetitions := Some d
                | Some accu -> // new digit -> shift left accumulator and add
                    repetitions := Some (accu * 10 + d)
                [] // not a digit -> skip
            | Character c ->
                match !repetitions with
                | None -> // no repetitions -> just return character
                    [string c]
                | Some accu -> // repetitions -> replicate character
                    repetitions := None // reset accumulator
                    List.replicate accu (string c)
            | x ->
                eprintf "Invalid Format Character: '%c'" x
                [] // not a digit nor a character -> skip

        Seq.toList
        >> List.collect parseFormatAtom

    let private nativeEndianness =
        if BitConverter.IsLittleEndian then Little else Big

    let private dataEndianness =
        function
        | '!' | '>' -> Big
        | '<' -> Little
        | '@' | '=' -> Native
        | _ -> Native

    let private stripEndiannessChar formatList =
        match formatList with
        | [] -> []
        | head :: tail ->
            match head with
            | "@" | "=" | "<" | ">" | "!" -> tail
            | _ -> head :: tail

    let private chooseEndianness machineOrder dataOrder =
        match machineOrder, dataOrder with
        | Big, Little
        | Little, Big -> adjustEndianNess
        | _, _ -> id

    let private checkMatchingSize receivedLen calculatedLen =
        let (|Compare|_|) check (r, c) =
            if (check) r c then Some (r - c) else None

        match (receivedLen, calculatedLen) with
        | Compare (>) (diff) ->
            sprintf "Wrong format or I would expect %d bytes less" diff |> fail
        | Compare (<) (diff) ->
            sprintf "Wrong format or I would expect %d bytes more" -diff |> fail
        | _ -> ok receivedLen

    let private readAtom (streamReader : BinaryReader) =
        function
        | "b" ->       S8  <| streamReader.ReadSByte()
        | "B" ->       U8  <| streamReader.ReadByte()
        | "h" ->       S16 <| streamReader.ReadInt16()
        | "H" ->       U16 <| streamReader.ReadUInt16()
        | "i" | "l" -> S32 <| streamReader.ReadInt32()
        | "I" | "L" -> U32 <| streamReader.ReadUInt32()
        | "f" ->       F32 <| streamReader.ReadSingle()
        | "d" ->       F64 <| streamReader.ReadDouble()
        | "q" ->       S64 <| streamReader.ReadInt64()
        | "Q" ->       U64 <| streamReader.ReadUInt64()
        |  _  -> failwith "Unknow Format Character"

    let calcsize fmt =
        let sizeOfAtom =
            function
            | "b"       -> sizeof<sbyte>
            | "B"       -> sizeof<byte>
            | "h"       -> sizeof<int16>
            | "H"       -> sizeof<uint16>
            | "i" | "l" -> sizeof<int32>
            | "I" | "L" -> sizeof<uint32>
            | "f"       -> sizeof<single>
            | "d"       -> sizeof<double>
            | "q"       -> sizeof<int64>
            | "Q"       -> sizeof<uint64>
            |  _ -> 0

        fmt
        |> explodeFormatString
        |> List.map sizeOfAtom
        |> List.reduce (+)

    let unpack (fmt : string) (data : byte []) =

        let _unpack (dataLen : int) =
            use dataStream = new MemoryStream(dataLen)
            dataStream.Write(data, 0, dataLen) |> ignore
            dataStream.Seek(0L, SeekOrigin.Begin) |> ignore
            use reader = new BinaryReader(dataStream)

            let unpackData =
                let convertAtom = readAtom reader
                let adjustEndianness = chooseEndianness nativeEndianness (dataEndianness fmt.[0])

                List.map (convertAtom >> adjustEndianness)

            fmt
            |> explodeFormatString
            |> stripEndiannessChar
            |> unpackData

        checkMatchingSize data.Length (calcsize fmt)
        |> lift _unpack
