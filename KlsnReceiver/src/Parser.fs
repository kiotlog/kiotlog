namespace KlsnReceiver

open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp

module SnPacket =

    CompositeResolver.RegisterAndSetAsDefault(
        FSharpResolver.Instance,
        StandardResolver.Instance
    )

    [<MessagePackObject>]
    type SnPacket = {
        [<Key("nonce")>]
        Nonce: byte []
        [<Key("data")>]
        Data: byte []
    }

    let parseSnPacket (msg : byte []) =
        MessagePackSerializer.Deserialize<SnPacket>(msg)
