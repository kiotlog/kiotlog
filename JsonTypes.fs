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

namespace KiotlogDBF.Models

open System
open System.ComponentModel.DataAnnotations.Schema

[<AutoOpen>]
module JsonTypes =

    [<AllowNullLiteral>]
    [<NotMapped>]
    type DevicesMeta () =
        member val Name = null with get, set
        member val Description = null with get, set
        member val Kind = null with get, set

    [<AllowNullLiteral>]
    [<NotMapped>]
    type DevicesAuth () =
        member val Basic : BasicAuth = null with get, set
        member val Klsn : KlsnAuth = null with get, set

    and [<AllowNullLiteral>]
        [<NotMapped>]
        BasicAuth () =
        member val Token : string = null with get, set

    and [<AllowNullLiteral>]
        [<NotMapped>]
        KlsnAuth () =
        member val Key : string = null with get, set

    [<AllowNullLiteral>]
    [<NotMapped>]
    type DevicesFrame () =
        member val Bigendian = false with get, set
        member val Bitfields = false with get, set

    [<AllowNullLiteral>]
    [<NotMapped>]
    type SensorsFmt () =
        member val Index: int = 0 with get, set
        member val FmtChr: string = null with get, set

    [<AllowNullLiteral>]
    [<NotMapped>]
    type SensorsMeta () =
        member val Name : string = null with get, set
        member val Description : string = null with get, set

    [<AllowNullLiteral>]
    [<NotMapped>]
    type SensorTypesMeta () =
        member val Max : int = Int32.MaxValue with get, set
        member val Min : int = Int32.MinValue with get, set
        
    [<AllowNullLiteral>]
    [<NotMapped>]
    type UserMeta () =
        member val Notes = String.Empty with get, set
    
    [<AllowNullLiteral>]
    [<NotMapped>]
    type UserAuth () =
        member val Passwd : string = null with get, set

    [<AllowNullLiteral>]
    [<NotMapped>]
    type TenantMeta () =
        member val Address = String.Empty with get, set
        member val Notes = String.Empty with get, set
