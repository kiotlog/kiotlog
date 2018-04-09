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

open KiotlogDBF.Json

[<AllowNullLiteral>]
type Devices () =

    member val Id = Guid.Empty with get, set
    member val Device = String.Empty with get, set
    member val Meta : DevicesMeta = null with get, set
    member val Auth : DevicesAuth = null with get, set
    member val Frame : DevicesFrame = null with get, set

    member val Points = HashSet<Points>() with get, set
    member val Sensors = HashSet<Sensors>() with get, set

    override this.ToString() =
        toJsonString this

    static member public ShouldSerializePoints () = false
    member public this.ShouldSerializeSensors () = not (Seq.isEmpty this.Sensors)

and [<AllowNullLiteral>]
    Points () =

    member val Id = Guid.Empty with get, set
    member val DeviceId = Guid.Empty with get, set
    member val Time = DateTime.Now with get, set
    member val Flags = "{}" with get, set
    member val Data = "{}" with get, set

    member val Device : Devices = null with get, set

    override this.ToString() =
        toJsonString this

and [<AllowNullLiteral>]
    Sensors() =

    member val Id = Guid.Empty with get, set
    member val Meta : SensorsMeta = null with get, set
    member val Fmt : SensorsFmt = null with get, set
    member val DeviceId = Guid.Empty with get, set
    member val SensorTypeId = Guid.Empty with get, set
    member val ConversionId = Guid.Empty with get, set

    member val Device : Devices = null with get, set
    member val SensorType : SensorTypes = null with get, set
    member val Conversion : Conversions = null with get, set

    override this.ToString() =
        toJsonString this

and [<AllowNullLiteral>]
    SensorTypes() =

    member val Meta : SensorTypesMeta = null with get, set
    member val Id = Guid.Empty with get, set
    member val Name = String.Empty with get, set
    member val Type = String.Empty with get, set

    member val Sensors = HashSet<Sensors>() with get, set

    static member public ShouldSerializeSensors () = false

    override this.ToString() =
        toJsonString this

and [<AllowNullLiteral>]
    Conversions() =

    member val Id = Guid.Empty with get, set
    member val Fun = String.Empty with get, set

    member val Sensors = HashSet<Sensors>() with get, set

    static member public ShouldSerializeSensors () = false
