// Learn more about F# at http://fsharp.org

open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp

open Decoder.Struct

open Chessie.ErrorHandling

CompositeResolver.RegisterAndSetAsDefault(
  FSharpResolver.Instance,
  StandardResolver.Instance
)

module Utils =

    open System

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
        |> Array.map (fun byte -> sprintf "%02x" byte)
        |> Array.reduce (+)

module Data =

    [<MessagePackObject>]
    type Packet = {
        [<Key("nonce")>]
        Nonce: byte []
        [<Key("data")>]
        Data: byte []
    }

open Utils
open Data
open Sodium

[<EntryPoint>]
let main argv =

    let key = "0102030405060708090A0B0C0D0E0F10C9CACBCCCDCECFD0D1D2D3D4D5D6D7D8" |> byteArrayFromHexString

    let msg = argv.[0] |> byteArrayFromHexString

    let recvData = MessagePackSerializer.Deserialize<Packet>(msg)

    printfn "Nonce is: %A" recvData.Nonce
    printfn "Msg is: %A" recvData.Data

    let plain = SecretAeadIETF.Decrypt(recvData.Data, recvData.Nonce, key)

    let unpacked = unpack "<hHHH" plain

    match unpacked with
    | Ok (x, _) -> printfn "Clear Text is: %A" x
    | Bad msgs -> printfn "Unable to parse %A" msgs
    0
