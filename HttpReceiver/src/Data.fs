namespace HttpReceiver

open System

module Data =

    module Payload =

        type Payload =
            {
                Name: string
                Time: string
                Msg: string
            }

        let touch msg x =
            let now = DateTime.Now
            let time = now.ToLongDateString() + " " + now.ToLongTimeString()
            { x with
                Name = x.Name + "d"
                Time = time
                Msg = msg}

        type Payload with member x.Touch (msg) = touch msg x
