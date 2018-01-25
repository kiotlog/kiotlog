open System
open System.Security.Cryptography

let rngCsp = new RNGCryptoServiceProvider()
let mutable randomNumber : byte [] = Array.zeroCreate 32
rngCsp.GetBytes(randomNumber)

let klsnKey = randomNumber |> Convert.ToBase64String
printfn "Base64:\n  %s" klsnKey

let randomNumberCInclude =
    randomNumber
    |> Array.map (sprintf "0x%02x")
    |> Array.chunkBySize 8
    |> Array.map (String.concat ", " >> sprintf "  %s")
    |> String.concat "\n"

printfn "C include:\n%s" randomNumberCInclude