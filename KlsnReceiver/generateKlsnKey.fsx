open System
open System.Security.Cryptography

let rngCsp = new RNGCryptoServiceProvider()
let mutable randomNumber : byte [] = Array.zeroCreate 32
rngCsp.GetBytes(randomNumber)

let klsnKey = randomNumber |> Convert.ToBase64String
printfn "%s" klsnKey
