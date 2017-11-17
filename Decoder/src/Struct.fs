namespace Decoder

open System
open System.IO
open System.Net

module PackedValue =

    type PackedValue =
        | S8 of sbyte
        | U8 of byte
        | S16 of int16
        | U16 of uint16
        | S32 of int32
        | U32 of uint32
        | S64 of int64
        | U64 of uint64

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
        | S8 value -> int16 value
        | U8 value -> int16 value
        | _ -> failwith "Value of incompatible type"

    let toUInt16 x =
        match x with
        | U16 value -> value
        | S8 value -> uint16 value
        | U8 value -> uint16 value
        | _ -> failwith "Value of incompatible type"

    let toInt32 x =
        match x with
        | S32 value -> value
        | S16 value -> int32 value
        | U16 value -> int32 value
        | S8 value -> int32 value
        | U8 value -> int32 value
        | _ -> failwith "Value of incompatible type"

    let toUInt32 x =
        match x with
        | U32 value -> value
        | S16 value -> uint32 value
        | U16 value -> uint32 value
        | S8 value -> uint32 value
        | U8 value -> uint32 value
        | _ -> failwith "Value of incompatible type"

    let toInt64 x =
        match x with
        | S64 value -> value
        | S32 value -> int64 value
        | U32 value -> int64 value
        | S16 value -> int64 value
        | U16 value -> int64 value
        | S8 value -> int64 value
        | U8 value -> int64 value
        | _ -> failwith "Value of incompatible type"

    let toUInt64 x =
        match x with
        | U64 value -> value
        | S32 value -> uint64 value
        | U32 value -> uint64 value
        | S16 value -> uint64 value
        | U16 value -> uint64 value
        | S8 value -> uint64 value
        | U8 value -> uint64 value
        | _ -> failwith "Value of incompatible type"

    let toFloat x =
        match x with
        | S64 value -> float value
        | U64 value -> float value
        | S32 value -> float value
        | U32 value -> float value
        | S16 value -> float value
        | U16 value -> float value
        | S8 value -> float value
        | U8 value -> float value

    let adjustEndianNess x =
        match x with
        | S8 _ | U8 _ -> x
        | S16 value -> value |> IPAddress.NetworkToHostOrder |> S16
        | U16 value -> value |> int16 |> IPAddress.NetworkToHostOrder |> uint16 |> U16
        | S32 value -> value |> IPAddress.NetworkToHostOrder |> S32
        | U32 value -> value |> int32 |> IPAddress.NetworkToHostOrder |> uint32 |> U32
        | S64 value -> value |> IPAddress.NetworkToHostOrder |> S64
        | U64 value -> value |> int64 |> IPAddress.NetworkToHostOrder |> uint64 |> U64

    type PackedValue with
        member x.ToSByte () = toSByte x
        member x.ToByte () = toByte x
        member x.ToInt16 () = toInt16 x
        member x.ToUInt16 () = toUInt16 x
        member x.ToInt32 () = toInt32 x
        member x.ToUInt32 () = toUInt32 x
        member x.ToInt64 () = toInt64 x
        member x.ToUInt64 () = toUInt64 x
        member x.ToFloat () = toFloat x
        member x.AdjustEndianNess () = adjustEndianNess x

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
           let mutable intvalue = 0
           if System.Int32.TryParse(string ch, &intvalue) then Some(intvalue)
           else None

        let (|Character|_|) (ch: char) =
            match ch with
            | '@' | '=' | '<' | '>' | '!'
            | 'b' | 'B'
            | 'h' | 'H'
            | 'i' | 'I' | 'l' | 'L'
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
            | _ -> failwith "Invalid Format Character"

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
            sprintf "Wrong format or I would expect %d bytes less" diff |> failwith
        | Compare (<) (diff) ->
            sprintf "Wrong format or I would expect %d bytes more" -diff |> failwith
        | _ -> ()

    let private readAtom (streamReader : BinaryReader) =
        function
        | "b" -> S8 <| streamReader.ReadSByte()
        | "B" -> U8 <| streamReader.ReadByte()
        | "h" -> S16 <| streamReader.ReadInt16()
        | "H" -> U16 <| streamReader.ReadUInt16()
        | "i" | "l" -> S32 <| streamReader.ReadInt32()
        | "I" | "L" -> U32 <| streamReader.ReadUInt32()
        | "q" -> S64 <| streamReader.ReadInt64()
        | "Q" -> U64 <| streamReader.ReadUInt64()
        |  _  -> failwith "Unknow Format Character"

    let calcsize fmt =
        let sizeOfAtom =
            function
            | "b" -> sizeof<sbyte>
            | "B" -> sizeof<byte>
            | "h" -> sizeof<int16>
            | "H" -> sizeof<uint16>
            | "i" | "l" -> sizeof<int32>
            | "I" | "L" -> sizeof<uint32>
            | "q" -> sizeof<int64>
            | "Q" -> sizeof<uint64>
            |  _ -> 0

        fmt
        |> explodeFormatString
        |> List.map sizeOfAtom
        |> List.reduce (+)

    let unpack (fmt : string) (data : byte []) : list<PackedValue> =

        let dataLen = data.Length

        checkMatchingSize dataLen (calcsize fmt) |> ignore

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
