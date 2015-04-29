namespace Pfim.Bench

open System.IO
open PerfUtil

module Tests =
  type Marker = class end

  let load x = File.ReadAllBytes(Path.Combine("data", x))

  [<PerfTest(50)>]
  let ``large targa`` (d : Decoder) =
    DevILSharp.Bootstrap.Init()
    let data = load "true-32-rle-large.tga"
    d.decode data ImageType.Targa

//  [<PerfTest(200)>]
//  let ``24bit targa`` (d : Decoder) =
//    DevILSharp.Bootstrap.Init()
//    let data = load "true-24.tga"
//    d.decode data ImageType.Targa
//
//  [<PerfTest(200)>]
//  let ``32bit run-length targa`` (d : Decoder) =
//    DevILSharp.Bootstrap.Init()
//    let data = load "true-32-rle.tga"
//    d.decode data ImageType.Targa
