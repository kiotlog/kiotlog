// Learn more about F# at http://fsharp.org

open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp

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

    [<MessagePackObject(true)>]
    type Packet = {
        Nonce: byte []
        Msg: byte []
    }

open Utils
open Data
open Sodium
open System.Text

[<EntryPoint>]
let main argv =

    printfn "Hello World from F#!"

    let key = "0102030405060708090A0B0C0D0E0F10C9CACBCCCDCECFD0D1D2D3D4D5D6D7D8" |> byteArrayFromHexString
    let cipher = "38d710b32467dd89a2fc0d750dd34a5dd78c58eeb820b9129544ea5b" |> byteArrayFromHexString
    let nonce = "65666768696a6b6c6e6f7071" |> byteArrayFromHexString

    let data = { Nonce = nonce; Msg = cipher }

    let bin = MessagePackSerializer.Serialize(data)

    printfn "%s" ( bin |> hexStringFromByteArray)

    let recvData = MessagePackSerializer.Deserialize<Packet>(bin)

    printfn "Nonce is: %A" recvData.Nonce
    printfn "Msg is: %A" recvData.Msg

    let plain = SecretAeadIETF.Decrypt(recvData.Msg, recvData.Nonce, key)
    printfn "Clear Text is: %A" plain

    let toEncrypt = argv.[0] |> Encoding.UTF8.GetBytes

    let cipherFromArgv = SecretAeadIETF.Encrypt(toEncrypt, nonce, key)
    printfn "Cipher Message is: %A" cipherFromArgv
    printfn "Cipher Message is: %s" (cipherFromArgv |> hexStringFromByteArray)

    0 // return an integer exit code
