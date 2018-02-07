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

namespace KiotlogDBF

open System
open System.Collections.Generic
open System.ComponentModel.DataAnnotations
open System.ComponentModel.DataAnnotations.Schema
open Newtonsoft.Json

open KiotlogDBF.Utils

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
    let mutable _meta = String.Empty

    [<Column("auth", TypeName = "jsonb")>]
    let mutable _auth = String.Empty

    [<Column("frame", TypeName = "jsonb")>]
    let mutable _frame = String.Empty

    [<NotMapped>]
    let mutable points = Unchecked.defaultof<ICollection<Points>>

    [<Column("id")>]
    member val Id = Guid.NewGuid() with get, set

    [<Column("device")>]
    [<Required(ErrorMessage="The Device field is Required")>]
    member val Device = String.Empty with get, set

    [<NotMapped>]
    member public __.Meta
        with get () = JsonConvert.DeserializeObject<DevicesMeta>(_meta)
        and  set(value: DevicesMeta) = _meta <- JsonConvert.SerializeObject(value)

    [<NotMapped>]
    member __.Auth
        with get () = JsonConvert.DeserializeObject<DevicesAuth>(_auth)
        and  set(value: DevicesAuth) = _auth <- JsonConvert.SerializeObject(value)

    [<NotMapped>]
    member __.Frame
        with get () = JsonConvert.DeserializeObject<DevicesFrame>(_frame)
        and  set(value: DevicesFrame) = _frame <- JsonConvert.SerializeObject(value)

    [<InverseProperty("Device")>]
    member __.Points
        with get() = points
        and set(value) = points <- value

    [<NotMapped>]
    [<DefaultValue>] val mutable internal sensors : ICollection<Sensors>
    // [<NotNullOrEmptyCollection(ErrorMessage="We need at leat one Sensor")>]
    [<InverseProperty("Device")>]
    member this.Sensors
        with get() = this.sensors
        and set(value) = this.sensors <- value

    override this.ToString() =
        toJsonString this

    static member public ShouldSerializemeta () = false
    static member public ShouldSerializeauth () = false
    static member public ShouldSerializeframe () = false
    static member public ShouldSerializesensors () = false
    static member public ShouldSerializepoints () = false
    static member public ShouldSerializePoints () = false

and [<AllowNullLiteral>]
    [<Table("points")>]
    Points() =

    [<Column("id")>]
    member val Id = Guid.NewGuid() with get, set

    [<Column("device_id")>]
    member val DeviceId = Guid.NewGuid() with get, set

    [<Column("time", TypeName = "timestamptz")>]
    member val Time = DateTime.Now with get, set

    [<Required>]
    [<Column("flags", TypeName = "jsonb")>]
    member val Flags = "{}" with get, set

    [<Required>]
    [<Column("data", TypeName = "jsonb")>]
    member val Data = "{}" with get, set

    [<NotMapped>]
    [<DefaultValue>] val mutable internal device : Devices
    [<ForeignKey("DeviceId")>]
    [<InverseProperty("Points")>]
    member this.Device
        with get() = this.device
        and set(value) = this.device <- value

    override this.ToString() =
        toJsonString this

and [<AllowNullLiteral>]
    [<Table("sensors")>]
    Sensors() =

    [<Column("meta", TypeName = "jsonb")>]
    let mutable _meta = String.Empty

    [<Column("fmt", TypeName = "jsonb")>]
    let mutable _fmt = String.Empty

    [<Column("id")>]
    member val Id = Guid.NewGuid() with get, set

    [<NotMapped>]
    member public __.Meta
        with get () = JsonConvert.DeserializeObject<SensorsMeta>(_meta, snakeSettings)
        and  set(value: SensorsMeta) = _meta <- JsonConvert.SerializeObject(value, snakeSettings)

    [<NotMapped>]
    member public __.Fmt
        with get () = JsonConvert.DeserializeObject<SensorsFmt>(_fmt, snakeSettings)
        and  set(value: SensorsFmt) = _fmt <- JsonConvert.SerializeObject(value, snakeSettings)

    [<Column("device_id")>]
    member val DeviceId = Guid.NewGuid() with get, set

    [<Column("sensor_type_id")>]
    member val SensorTypeId = Guid.NewGuid() with get, set

    [<Column("conversion_id")>]
    member val ConversionId = Guid.NewGuid() with get, set

    [<NotMapped>]
    [<DefaultValue>] val mutable internal device : Devices
    [<ForeignKey("DeviceId")>]
    [<InverseProperty("Sensors")>]
    member this.Device
        with get() = this.device
        and set(value) = this.device <- value

    [<NotMapped>]
    [<DefaultValue>] val mutable internal sensortype : SensorTypes
    [<ForeignKey("SensorTypeId")>]
    [<InverseProperty("Sensors")>]
    member this.SensorType
        with get() = this.sensortype
        and set(value) = this.sensortype <- value

    [<NotMapped>]
    [<DefaultValue>] val mutable internal conversion : Conversions
    [<ForeignKey("ConversionId")>]
    [<InverseProperty("Sensors")>]
    member this.Conversion
        with get() = this.conversion
        and set(value) = this.conversion <- value

    static member public ShouldSerializemeta () = false
    static member public ShouldSerializefmt () = false
    static member public ShouldSerializesensortype () = false
    static member public ShouldSerializeconversion () = false

    override this.ToString() =
        toJsonString this

and [<AllowNullLiteral>]
    [<Table("sensor_types")>]
    SensorTypes() =

    [<Column("meta", TypeName = "jsonb")>]
    let mutable _meta = String.Empty

    [<Column("id")>]
    member val Id = Guid.NewGuid() with get, set

    [<Required>]
    [<Column("name")>]
    member val Name = String.Empty with get, set

    [<Required>]
    [<Column("type")>]
    member val Type = String.Empty with get, set

    [<NotMapped>]
    member public __.Meta
        with get () = JsonConvert.DeserializeObject<SensorTypesMeta>(_meta)
        and  set(value: SensorTypesMeta) = _meta <- JsonConvert.SerializeObject(value)

    [<NotMapped>]
    [<DefaultValue>] val mutable internal sensors : ICollection<Sensors>
    [<InverseProperty("SensorType")>]
    member this.Sensors
        with get() = this.sensors
        and set(value) = this.sensors <- value

    static member public ShouldSerializemeta () = false
    static member public ShouldSerializesensors () = false
    static member public ShouldSerializeSensors () = false

    override this.ToString() =
        toJsonString this

and [<AllowNullLiteral>]
    [<Table("conversions")>]
    Conversions() =

    [<Column("id")>]
    member val Id = Guid.NewGuid() with get, set

    [<Required>]
    [<Column("fun")>]
    member val Fun = String.Empty with get, set

    [<NotMapped>]
    [<DefaultValue>] val mutable internal sensors : ICollection<Sensors>
    [<InverseProperty("Conversion")>]
    member this.Sensors
        with get() = this.sensors
        and set(value) = this.sensors <- value

    static member public ShouldSerializesensors () = false
    static member public ShouldSerializeSensors () = false
