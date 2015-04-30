namespace Pfim.Bench

open Pfim
open System.IO
open PerfUtil
open ImageMagick

type ImageType =
| Dds = 1
| Targa = 2

[<AbstractClass>]
type Decoder() =
  abstract Name : string
  abstract decode : byte[] -> ImageType -> unit

  interface ITestable with
    member x.Fini() = ()
    member x.Init() = ()
    member x.Name = x.Name
 
type PfimDecoder () =
  inherit Decoder ()

  override __.Name = "Pfim"
  override __.decode data typ =
    match typ with
    | ImageType.Dds -> Pfim.Dds.Create(new MemoryStream(data)) |> ignore
    | ImageType.Targa -> Pfim.Targa.Create(new MemoryStream(data)) |> ignore
    | _ -> failwith "Not supported"

type DevilDecoder () =
  inherit Decoder ()

  override __.Name = "DevIL"
  override __.decode data typ =
    match typ with
    | ImageType.Dds -> failwith "not all"
    | ImageType.Targa -> DevILSharp.Image.Load(data, DevILSharp.ImageType.Tga) |> ignore
    | _ -> failwith "Not supported"

type TargaDecoder () =
  inherit Decoder ()

  override __.Name = "TargaImage"
  override __.decode data typ =
    match typ with
    | ImageType.Dds -> failwith "not all"
    | ImageType.Targa ->
      let str = new MemoryStream(data) :> Stream
      using(new Paloma.TargaImage(str)) (fun x -> ())
    | _ -> failwith "Not supported"

type ImageMagickDecoder () =
  inherit Decoder ()

  override __.Name = "ImageMagick"
  override __.decode data typ =
    match typ with
    | ImageType.Dds -> failwith "not all"
    | ImageType.Targa ->
      let settings = MagickReadSettings()
      settings.Format <- System.Nullable MagickFormat.Tga
      new MagickImage(new MemoryStream(data), settings) |> ignore
    | _ -> failwith "Not supported"

type FreeImageDecoder () =
  inherit Decoder ()

  override __.Name = "FreeImage"
  override __.decode data typ =
    match typ with
    | ImageType.Dds -> failwith "not all"
    | ImageType.Targa ->
      FreeImageAPI.FreeImageBitmap.FromStream(new MemoryStream(data)).ToBitmap() |> ignore
    | _ -> failwith "Not supported"

type DecoderPerf =
  static member CreateComparer () =
    let this = new PfimDecoder() :> Decoder
    let others = [ new DevilDecoder() :> Decoder
                   new TargaDecoder() :> Decoder
                   new FreeImageDecoder() :> Decoder ]
    new ImplementationComparer<Decoder>(this, others, warmup = false)