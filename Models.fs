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
open System.Collections.Generic
open System.ComponentModel.DataAnnotations
open System.ComponentModel.DataAnnotations.Schema
open Newtonsoft.Json

open KiotlogDBF.Utils
open System.Collections
open System.Collections.Generic
open System.Collections.Generic
open System.Collections.Generic

[<NotMapped>]
type DevicesMeta = {
    Name: string
    Description: string
    Kind: string
}

[<NotMapped>]
type DevicesAuth = {
    Basic: BasicAuth
    Klsn: KlsnAuth
}
and [<NotMapped>] BasicAuth = {
    Token: string
}
and [<NotMapped>] KlsnAuth = {
    Key: string
}

[<NotMapped>]
type DevicesFrame = {
    Bigendian: bool
    Bitfields: bool
}

[<NotMapped>]
type SensorsFmt = {
    Index: int
    FmtChr: string
}

[<NotMapped>]
type SensorsMeta = {
    Name: string
    Description: string
}

[<NotMapped>]
type SensorTypesMeta = {
    Max: int
    Min: int
}

[<AllowNullLiteral>]
[<Table("devices")>]
type Devices () =

    [<Column("meta", TypeName = "jsonb")>]
    member val internal _Meta = null with get, set

    [<Column("auth", TypeName = "jsonb")>]
    member val internal _Auth = null with get, set

    [<Column("frame", TypeName = "jsonb")>]
    member val internal _Frame = null with get, set

    [<Column("id")>]
    member val Id = Guid.Empty with get, set

    [<Column("device")>]
    [<Required(ErrorMessage="The Device field is Required")>]
    member val Device = String.Empty with get, set

    [<NotMapped>]
    member public this.Meta
        with get () =
            match this._Meta with
            | null -> { Name = null ; Description = null ; Kind = null }
            | _ -> JsonConvert.DeserializeObject<DevicesMeta>(this._Meta)
        and  set(value: DevicesMeta) = this._Meta <- JsonConvert.SerializeObject(value)

    [<NotMapped>]
    member this.Auth
        with get () =
            match this._Auth with
            | null -> { Basic = { Token = null }; Klsn = { Key = null }}
            | _ -> JsonConvert.DeserializeObject<DevicesAuth>(this._Auth)
        and  set (value: DevicesAuth) = this._Auth <- JsonConvert.SerializeObject(value)

    [<NotMapped>]
    member this.Frame
        with get () =
            match this._Frame with
            | null -> { Bigendian = true; Bitfields = false }
            | _ -> JsonConvert.DeserializeObject<DevicesFrame>(this._Frame)
        and  set (value: DevicesFrame) = this._Frame <- JsonConvert.SerializeObject(value)

    [<InverseProperty("Device")>]
    member val Points = HashSet<Points>() :> ICollection<_> with get, set

    [<InverseProperty("Device")>]
    member val Sensors = HashSet<Sensors>() :> ICollection<_> with get, set

    override this.ToString() =
        toJsonString this

    static member public ShouldSerialize_Meta () = false
    static member public ShouldSerialize_Auth () = false
    static member public ShouldSerialize_Frame () = false
    static member public ShouldSerializePoints () = false

and [<AllowNullLiteral>]
    [<Table("points")>]
    Points() =

    [<Column("id")>]
    member val Id = Guid.Empty with get, set

    [<Column("device_id")>]
    member val DeviceId = Guid.Empty with get, set

    [<Column("time", TypeName = "timestamptz")>]
    member val Time = DateTime.Now with get, set

    [<Required>]
    [<Column("flags", TypeName = "jsonb")>]
    member val Flags = "{}" with get, set

    [<Required>]
    [<Column("data", TypeName = "jsonb")>]
    member val Data = "{}" with get, set

    [<ForeignKey("DeviceId")>]
    [<InverseProperty("Points")>]
    member val Device = Devices() with get, set

    override this.ToString() =
        toJsonString this

and [<AllowNullLiteral>]
    [<Table("sensors")>]
    Sensors() =

    [<Column("meta", TypeName = "jsonb")>]
    member val internal _Meta = null with get, set

    [<Column("fmt", TypeName = "jsonb")>]
    member val internal _Fmt = null with get, set

    [<Column("id")>]
    member val Id = Guid.Empty with get, set

    [<NotMapped>]
    member public this.Meta
        with get () =
            match this._Meta with
            | null -> { Description = null ; Name = null }
            | _ -> JsonConvert.DeserializeObject<SensorsMeta>(this._Meta, snakeSettings)
        and  set (value: SensorsMeta) = this._Meta <- JsonConvert.SerializeObject(value, snakeSettings)

    [<NotMapped>]
    member public this.Fmt
        with get () =
            match this._Fmt with
            | null -> { FmtChr = "B"; Index = 0}
            | _ -> JsonConvert.DeserializeObject<SensorsFmt>(this._Fmt, snakeSettings)
        and  set (value: SensorsFmt) = this._Fmt <- JsonConvert.SerializeObject(value, snakeSettings)

    [<Column("device_id")>]
    member val DeviceId = Guid.Empty with get, set

    [<Column("sensor_type_id")>]
    member val SensorTypeId = Guid.Empty with get, set

    [<Column("conversion_id")>]
    member val ConversionId = Guid.Empty with get, set

    [<ForeignKey("DeviceId")>]
    [<InverseProperty("Sensors")>]
    member val Device = Devices() with get, set

    [<ForeignKey("SensorTypeId")>]
    [<InverseProperty("Sensors")>]
    member val SensorType = null :> SensorTypes with get, set

    [<ForeignKey("ConversionId")>]
    [<InverseProperty("Sensors")>]
    member val Conversion = null :> Conversions with get, set

    override this.ToString() =
        toJsonString this

and [<AllowNullLiteral>]
    [<Table("sensor_types")>]
    SensorTypes() =

    [<Column("meta", TypeName = "jsonb")>]
    member val _Meta = null with get, set

    [<Column("id")>]
    member val Id = Guid.Empty with get, set

    [<Required>]
    [<Column("name")>]
    member val Name = String.Empty with get, set

    [<Required>]
    [<Column("kind")>]
    member val Kind = String.Empty with get, set

    [<NotMapped>]
    member public this.Meta
        with get () =
            match this._Meta with
            | null -> { Max = 100; Min = 0 }
            | _ -> JsonConvert.DeserializeObject<SensorTypesMeta>(this._Meta)
        and  set(value: SensorTypesMeta) = this._Meta <- JsonConvert.SerializeObject(value)

    [<InverseProperty("SensorType")>]
    member val Sensors = HashSet<Sensors>() :> ICollection<_> with get, set

    static member public ShouldSerialize_Meta () = false
    static member public ShouldSerializeSensors () = false

    override this.ToString() =
        toJsonString this

and [<AllowNullLiteral>]
    [<Table("conversions")>]
    Conversions() =

    [<Column("id")>]
    member val Id = Guid.Empty with get, set

    [<Required>]
    [<Column("fun")>]
    member val Fun = String.Empty with get, set

    [<InverseProperty("Conversion")>]
    member val Sensors = HashSet<Sensors>() :> ICollection<_> with get, set

    static member public ShouldSerializeSensors () = false
