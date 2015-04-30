#cd "../../bin/Pfim.Bench"
#r "../../bin/Pfim.Bench/PerfUtil.dll"
#r "../../bin/Pfim.Bench/Pfim.Bench.dll"

open System
open System.IO

// DevIL requires the working directory to be the directory that it is in so
// that it can extract the embedded binaries
let cwd = Path.Combine(__SOURCE_DIRECTORY__, "..", "..", "bin", "Pfim.Bench")
System.Environment.CurrentDirectory <- cwd

open PerfUtil
open Pfim.Bench

let perfResults =
  PerfTest.OfModuleMarker<Tests.Marker>()
  |> PerfTest.run (fun () -> DecoderPerf.CreateComparer () :> _)

let file = File.CreateText("benchmark.csv")
file.WriteLine("Test,DevIL,ImageMagick,Pfim,TargaImage")

perfResults
|> List.collect (fun x ->
  x.Results
  |> Map.toList
  |> List.map (fun (key, va) -> (key, va.SessionId, va.Elapsed.TotalSeconds)))
|> List.toSeq
|> Seq.groupBy(fun (key, session, elapsed) -> key)
|> Seq.map (fun (key, vals) ->
  // Keys are in the format of "Test.<test name>". We want <test name>
  let newKey = key.Substring(key.IndexOf('.') + 1)

  // Extact just the elapsed time (sorted alphabetically by library)
  let nvals =
    vals
    |> Seq.map(fun (_, session, elapsed) -> (session, elapsed))
    |> Seq.sortBy fst
    |> Seq.map snd

  newKey, nvals)
|> Seq.iter (fun (key, vals) ->
  // Export data to csv with the relative times to complete decoding
  file.Write(key)
  file.Write(',')
  let min = vals |> Seq.min
  vals
  |> Seq.map (fun x -> (x / min).ToString("0.000"))
  |> String.concat ","
  |> fun v -> file.WriteLine(v))
file.Flush()
file.Close()

printfn "%A" perfResults