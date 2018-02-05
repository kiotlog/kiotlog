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

type [<Table("devices")>] Devices () =

    [<Column("id")>]
    [<DefaultValue>] val mutable Id : Guid

    [<Column("device")>]
    [<Required(ErrorMessage="The Device field is Required")>]
    [<DefaultValue>] val mutable Device : string

    [<Column("meta", TypeName = "jsonb")>]
    [<DefaultValue>] val mutable meta : string
    [<NotMapped>]
    member public this.Meta
        with get () = JsonConvert.DeserializeObject<DevicesMeta>(this.meta)
        and  set(value: DevicesMeta) = this.meta <- JsonConvert.SerializeObject(value)

    [<Column("auth", TypeName = "jsonb")>]
    [<DefaultValue>] val mutable auth : string
    [<NotMapped>]
    member this.Auth
        with get () = JsonConvert.DeserializeObject<DevicesAuth>(this.auth)
        and  set(value: DevicesAuth) = this.auth <- JsonConvert.SerializeObject(value)

    [<Column("frame", TypeName = "jsonb")>]
    [<DefaultValue>] val mutable frame : string
    [<NotMapped>]
    member this.Frame
        with get () = JsonConvert.DeserializeObject<DevicesFrame>(this.frame)
        and  set(value: DevicesFrame) = this.frame <- JsonConvert.SerializeObject(value)

    [<NotMapped>]
    let mutable points = Unchecked.defaultof<ICollection<Points>>
    [<InverseProperty("Device")>]
    member __.Points
        with get() = points
        and set(value) = points <- value

    [<NotMapped>]
    [<DefaultValue>] val mutable sensors : ICollection<Sensors>
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

and [<Table("points")>] Points() =

    [<Column("id")>]
    [<DefaultValue>] val mutable Id : Guid

    [<Column("device_id")>]
    [<DefaultValue>] val mutable DeviceId : Guid

    [<Column("time", TypeName = "timestamptz")>]
    [<DefaultValue>] val mutable Time : DateTime

    [<Required>]
    [<Column("flags", TypeName = "jsonb")>]
    [<DefaultValue>] val mutable Flags : string

    [<Required>]
    [<Column("data", TypeName = "jsonb")>]
    [<DefaultValue>] val mutable Data : string

    [<NotMapped>]
    [<DefaultValue>] val mutable device : Devices
    [<ForeignKey("DeviceId")>]
    [<InverseProperty("Points")>]
    member this.Device
        with get() = this.device
        and set(value) = this.device <- value

    override this.ToString() =
        toJsonString this

and [<Table("sensors")>] Sensors() =

    [<Column("id")>]
    [<DefaultValue>] val mutable Id : Guid

    [<Column("meta", TypeName = "jsonb")>]
    [<DefaultValue>] val mutable meta : string
    [<NotMapped>]
    member public this.Meta
        with get () = JsonConvert.DeserializeObject<SensorsMeta>(this.meta, snakeSettings)
        and  set(value: SensorsMeta) = this.meta <- JsonConvert.SerializeObject(value, snakeSettings)

    [<Column("fmt", TypeName = "jsonb")>]
    [<DefaultValue>] val mutable fmt : string
    [<NotMapped>]
    member public this.Fmt
        with get () = JsonConvert.DeserializeObject<SensorsFmt>(this.fmt, snakeSettings)
        and  set(value: SensorsFmt) = this.fmt <- JsonConvert.SerializeObject(value, snakeSettings)

    [<Column("device_id")>]
    [<DefaultValue>] val mutable DeviceId : Guid

    [<Column("sensor_type_id")>]
    [<DefaultValue>] val mutable SensorTypeId : Guid

    [<Column("conversion_id")>]
    [<DefaultValue>] val mutable ConversionId : Guid

    [<NotMapped>]
    [<DefaultValue>] val mutable device : Devices
    [<ForeignKey("DeviceId")>]
    [<InverseProperty("Sensors")>]
    member this.Device
        with get() = this.device
        and set(value) = this.device <- value

    [<NotMapped>]
    [<DefaultValue>] val mutable sensortype : SensorTypes
    [<ForeignKey("SensorTypeId")>]
    [<InverseProperty("Sensors")>]
    member this.SensorType
        with get() = this.sensortype
        and set(value) = this.sensortype <- value

    [<NotMapped>]
    [<DefaultValue>] val mutable conversion : Conversions
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

and [<Table("sensor_types")>] SensorTypes() =

    [<Column("id")>]
    [<DefaultValue>] val mutable Id : Guid

    [<Required>]
    [<Column("name")>]
    [<DefaultValue>] val mutable Name : string

    [<Required>]
    [<Column("type")>]
    [<DefaultValue>] val mutable Type : string

    [<Column("meta", TypeName = "jsonb")>]
    [<DefaultValue>] val mutable meta : string
    [<NotMapped>]
    member public this.Meta
        with get () = JsonConvert.DeserializeObject<SensorTypesMeta>(this.meta)
        and  set(value: SensorTypesMeta) = this.meta <- JsonConvert.SerializeObject(value)

    [<NotMapped>]
    [<DefaultValue>] val mutable sensors : ICollection<Sensors>
    [<InverseProperty("SensorType")>]
    member this.Sensors
        with get() = this.sensors
        and set(value) = this.sensors <- value

    static member public ShouldSerializemeta () = false
    static member public ShouldSerializesensors () = false
    static member public ShouldSerializeSensors () = false

    override this.ToString() =
        toJsonString this

and [<Table("conversions")>] Conversions() =

    [<Column("id")>]
    [<DefaultValue>] val mutable Id : Guid

    [<Required>]
    [<Column("fun")>]
    [<DefaultValue>] val mutable Fun : string

    [<NotMapped>]
    [<DefaultValue>] val mutable sensors : ICollection<Sensors>
    [<InverseProperty("Conversion")>]
    member this.Sensors
        with get() = this.sensors
        and set(value) = this.sensors <- value

    static member public ShouldSerializesensors () = false
    static member public ShouldSerializeSensors () = false
