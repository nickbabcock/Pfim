namespace Pfim.Bench

open System.IO
open PerfUtil

module Tests =
  type Marker = class end

  let load x = File.ReadAllBytes(Path.Combine("data", x))

  do
    DevILSharp.Bootstrap.Init();

  [<PerfTest(50)>]
  let ``large targa`` (d : Decoder) =
    let data = load "true-32-rle-large.tga"
    d.decode data ImageType.Targa
