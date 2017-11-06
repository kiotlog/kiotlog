module Trmpln.Units

[<Measure>] type V
[<Measure>] type mV

[<Measure>] type C
[<Measure>] type dC

[<Measure>] type hPa
[<Measure>] type daPa
[<Measure>] type Pa

let mvPerV : float<mV V^-1> = 1000.0<mV/V>
let dCPerC : float<dC C^-1> = 10.0<dC/C>
let daPaPerPa : float<daPa Pa^-1> = 0.1<daPa/Pa>
let hPaPerPa : float<hPa Pa^-1> = 0.01<hPa/Pa>

let inline convertmVToV (x: float<mV>): float<V> = x / mvPerV
let inline convertdCToC (x: float<dC>) = x / dCPerC
let inline convertdaPaTohPa (x: float<daPa>) = x / daPaPerPa * hPaPerPa

let inline toVolt x =
    x * 1.0<V>

let inline tomVolt x =
    x * 1.0<mV>

let inline toCelsius x =
    x * 1.0<C>

let inline todCelsius x =
    x * 1.0<dC>

let inline toPascal x =
    x * 1.0<Pa>

let inline tohPascal x =
    x * 1.0<hPa>

let inline todaPascal x =
    x * 1.0<daPa>