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
