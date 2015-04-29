#cd "../../bin/Pfim.Bench"
#r "../../bin/Pfim.Bench/PerfUtil.dll"
#r "../../bin/Pfim.Bench/Pfim.Bench.dll"

open System
open System.IO

System.Environment.CurrentDirectory <- Path.Combine(__SOURCE_DIRECTORY__, "..", "..", "bin", "Pfim.Bench");

open PerfUtil
open Pfim.Bench

// Define your library scripting code here

let perfResults =
  PerfTest.OfModuleMarker<Tests.Marker>()
  |> PerfTest.run (fun () -> DecoderPerf.CreateComparer () :> _)

let file = File.CreateText("temp.csv")
file.WriteLine("Test,DevIL,Pfim,TargaImage")

perfResults
|> List.collect (fun x ->
  x.Results
  |> Map.toList
  |> List.map (fun (key, va) -> (key, va.SessionId, va.Elapsed.TotalSeconds)))
|> List.toSeq
|> Seq.groupBy(fun (key, session, elapsed) -> key)
|> Seq.map (fun (key, vals) ->
  let newKey = key.Substring(key.IndexOf('.') + 1)
  let nvals =
    vals
    |> Seq.map(fun (_, session, elapsed) -> (session, elapsed))
    |> Seq.sortBy fst
    |> Seq.map snd

  newKey, nvals)
|> Seq.iter (fun (key, vals) ->
  file.Write(key)
  file.Write(',')
  vals |> Seq.iter (fun x -> file.Write(x); file.Write(','))
  file.WriteLine())
file.Flush()
file.Close()

printfn "%A" perfResults