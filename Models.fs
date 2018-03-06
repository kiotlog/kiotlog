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

open KiotlogDBF.Json

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
    member val Name: string = null with get, set
    member val Description: string = null with get, set

[<AllowNullLiteral>]
[<NotMapped>]
type SensorTypesMeta () =
    member val Max: int = Int32.MaxValue with get, set
    member val Min: int = Int32.MinValue with get, set

[<AllowNullLiteral>]
[<Table("devices")>]
type Devices () =

    [<Column("meta", TypeName = "jsonb")>]
    member val Meta : DevicesMeta = null with get, set

    [<Column("auth", TypeName = "jsonb")>]
    member val Auth : DevicesAuth = null with get, set

    [<Column("frame", TypeName = "jsonb")>]
    member val Frame : DevicesFrame = null with get, set

    [<Column("id")>]
    member val Id = Guid.Empty with get, set

    [<Column("device")>]
    [<Required(ErrorMessage="The Device field is Required")>]
    member val Device = String.Empty with get, set

    [<InverseProperty("Device")>]
    member val Points = HashSet<Points>() :> ICollection<_> with get, set

    [<InverseProperty("Device")>]
    member val Sensors = HashSet<Sensors>() :> ICollection<_> with get, set

    override this.ToString() =
        toJsonString this

    // static member public ShouldSerialize_Meta () = false
    static member public ShouldSerialize_Auth () = false
    static member public ShouldSerialize_Frame () = false
    static member public ShouldSerializePoints () = false
    member public this.ShouldSerializeSensors () = not (Seq.isEmpty this.Sensors)

and [<AllowNullLiteral>]
    [<Table("points")>]
    Points () =

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
    member val Device : Devices = null with get, set

    override this.ToString() =
        toJsonString this

and [<AllowNullLiteral>]
    [<Table("sensors")>]
    Sensors() =

    [<Column("meta", TypeName = "jsonb")>]
    member val Meta : SensorsMeta = null with get, set

    [<Column("fmt", TypeName = "jsonb")>]
    member val Fmt : SensorsFmt = null with get, set

    [<Column("id")>]
    member val Id = Guid.Empty with get, set

    [<Column("device_id")>]
    member val DeviceId = Guid.Empty with get, set

    [<Column("sensor_type_id")>]
    member val SensorTypeId = Guid.Empty with get, set

    [<Column("conversion_id")>]
    member val ConversionId = Guid.Empty with get, set

    [<ForeignKey("DeviceId")>]
    [<InverseProperty("Sensors")>]
    member val Device : Devices = null with get, set

    [<ForeignKey("SensorTypeId")>]
    [<InverseProperty("Sensors")>]
    member val SensorType : SensorTypes = null with get, set

    [<ForeignKey("ConversionId")>]
    [<InverseProperty("Sensors")>]
    member val Conversion : Conversions = null with get, set

    override this.ToString() =
        toJsonString this

and [<AllowNullLiteral>]
    [<Table("sensor_types")>]
    SensorTypes() =

    [<Column("meta", TypeName = "jsonb")>]
    member val Meta : SensorTypesMeta = null with get, set

    [<Column("id")>]
    member val Id = Guid.Empty with get, set

    [<Required>]
    [<Column("name")>]
    member val Name = String.Empty with get, set

    [<Required>]
    [<Column("kind")>]
    member val Kind = String.Empty with get, set

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
