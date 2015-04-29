#r "bin/Release/PerfUtil.dll"
#r "bin/Release/Pfim.Bench.dll"

open PerfUtil
open Pfim.Bench

// Define your library scripting code here

let perfResults =
  PerfTest.OfModuleMarker<Tests.Marker>()
  |> PerfTest.run (fun () -> DecoderPerf.CreateComparer () :> _)

printfn "%A" perfResults