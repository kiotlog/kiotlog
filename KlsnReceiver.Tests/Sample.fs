module Tests

open KlsnReceiver.SnPacket

open Expecto
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp

CompositeResolver.RegisterAndSetAsDefault(
    FSharpResolver.Instance,
    StandardResolver.Instance
)

[<Tests>]
let tests =
    testList "SnPackets" [
        testCase "Parse SnPacket Simple" <| fun _ ->
            let packet =
                { Nonce = "Hello, World!" |> System.Text.Encoding.UTF8.GetBytes
                  Data =  [| 0uy; 1uy; 2uy; 3uy; 4uy; 5uy; 6uy; 7uy; 8uy; 9uy |] }

            let msg = MessagePackSerializer.Serialize(packet)
            let parsedMsg = parseSnPacket<SnPacket> msg
            Expect.equal parsedMsg packet "parsedMsg is msg"

        testCase "Parse SnPacket with Timestamp" <| fun _ ->
            let packetTs =
                { Nonce = "Hello, World!" |> System.Text.Encoding.UTF8.GetBytes
                  Data =  [| 0uy; 1uy; 2uy; 3uy; 4uy; 5uy; 6uy; 7uy; 8uy; 9uy |]
                  Timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() |> System.BitConverter.GetBytes  }

            let msg = MessagePackSerializer.Serialize(packetTs)
            let parsedMsg = parseSnPacket<SnPacketTs> msg
            Expect.equal parsedMsg packetTs "parsedMsg is msg"
    ]
