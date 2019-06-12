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

namespace Decoder

module Conversions =

    let int32ToFloat (value : float) (max : int) (min : int) : float =
        let conversionFactor = float ((1 <<< 31) - 1) / float (max - min)

        float value / conversionFactor

    let uint32ToFloat (value : float) (max : int) (min : int) : float =
        let conversionFactor = float ((1 <<< 32) - 1) / float (max - min)

        float value / conversionFactor

    let int16ToFloat (value : float) (max : int) (min : int) : float =
        let conversionFactor = float ((1 <<< 15) - 1) / float (max - min)

        float value / conversionFactor

    let uint16ToFloat (value : float) (max : int) (min : int) : float =
        let conversionFactor = float ((1 <<< 16) - 1) / float (max - min)

        float value / conversionFactor

    let int8ToFloat (value : float) (max : int) (min : int) : float =
        let conversionFactor = float ((1 <<< 7) - 1) / float (max - min)

        float value / conversionFactor

    let uint8ToFloat (value : float) (max : int) (min : int) : float =
        let conversionFactor = float ((1 <<< 8) - 1) / float (max - min)

        float value / conversionFactor

    let signIntNToFloat (sign: bool) (N: int) (value : float) (max : int) (min : int) : float =
        let bits = if sign then N - 1 else N
        let conversionFactor = float ((1 <<< bits) - 1) / float (max - min)

        float value / conversionFactor

    let xMul mul (value : float) (_ : int) (_ : int) : float =
        float value / mul

    let inline klConvert value max min fn =
        match fn with
        | "float_to_int32"  -> int32ToFloat    value max min
        | "float_to_uint32" -> uint32ToFloat   value max min
        | "float_to_int16"  -> int16ToFloat    value max min
        | "float_to_uint16" -> uint16ToFloat   value max min
        | "float_to_int8"   -> int8ToFloat     value max min
        | "float_to_uint8"  -> uint8ToFloat    value max min
        | "x10"             -> xMul        10. value max min
        | "x100"            -> xMul       100. value max min
        | "x1000"           -> xMul     1_000. value max min
        | "x10000"          -> xMul    10_000. value max min
        | "x100000"         -> xMul   100_000. value max min
        | "x1000000"        -> xMul 1_000_000. value max min
        | _ -> float value
