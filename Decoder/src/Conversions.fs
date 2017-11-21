namespace Decoder

module Conversions =

    let int16ToFloat (value : float) (max : int) (min : int) : float =
        let conversionFactor = float ((1 <<< 15) - 1) / float (max - min)
        
        float value / conversionFactor

    let uint16ToFloat (value : float) (max : int) (min : int) : float =
        let conversionFactor = float ((1 <<< 16) - 1) / float (max - min)
        
        float value / conversionFactor

    let xMul mul (value : float) (max : int) (min : int) : float =
        float value / mul
    
    let inline klConvert value max min fn =
        match fn with
        | "float_to_int16" -> int16ToFloat value max min
        | "float_to_uint16" -> uint16ToFloat value max min
        | "x10" -> xMul 10. value max min
        | "x100" -> xMul 100. value max min
        | _ -> float value