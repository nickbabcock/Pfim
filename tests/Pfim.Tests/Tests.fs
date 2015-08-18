module Pfim.Tests

open Pfim
open NUnit.Framework
open System.IO

let shouldEqual (x : 'a) (y : 'a) = Assert.AreEqual(x, y, sprintf "Expected: %A\nActual: %A" x y)

let toBytes arr = [| for i in arr -> byte(i) |]

[<Test>]
let ``translate identity`` () =
  let buf = [| 1uy..5uy |]
  use mem = new MemoryStream()
  let actual = Util.Translate(mem, buf, 0)
  actual |> shouldEqual 5
  buf |> shouldEqual [| 1uy..5uy |]

[<Test>]
let ``translate a byte from the stream`` () =
  let buf = [| 1uy..5uy|]
  use mem = new MemoryStream([|100uy|])
  let actual = Util.Translate(mem, buf, 1)
  actual |> shouldEqual 5
  buf |> shouldEqual [| 2uy; 3uy; 4uy; 5uy; 100uy; |]

[<Test>]
let ``translate a byte from the stream but it doesn't have a byte to give`` () =
  let buf = [| 1uy..5uy |]
  use mem = new MemoryStream()
  let actual = Util.Translate(mem, buf, 1)
  actual |> shouldEqual 4
  buf |> shouldEqual [| 2uy; 3uy; 4uy; 5uy; 5uy; |]

[<Test>]
let ``translate all but the last byte`` () =
  let buf = [| 1uy..5uy |]
  use mem = new MemoryStream([| for i in 100 .. -1 .. 97 -> byte(i)|])
  let actual = Util.Translate(mem, buf, 4)
  actual |> shouldEqual 5
  buf |> shouldEqual [| 5uy; 100uy; 99uy; 98uy; 97uy |]

[<Test>]
let ``translate all but the last byte but the stream doesn't have the bytes`` () =
  let buf = [| 1uy..5uy |]
  use mem = new MemoryStream()
  let actual = Util.Translate(mem, buf, 4)
  actual |> shouldEqual 1
  buf |> shouldEqual [| 5uy; 2uy; 3uy; 4uy; 5uy |]

[<Test>]
let ``fill bottom left with single pixel sized rows`` () =
  let mutable data = Array.zeroCreate 5
  use mem = new MemoryStream([| 1uy .. 5uy |])
  Util.FillBottomLeft(mem, data, 1)
  data |> shouldEqual (Array.rev [| 1uy .. 5uy |])

[<Test>]
let ``fill bottom left with double pixel sized rows`` () =
  let mutable data = Array.zeroCreate 6
  use mem = new MemoryStream([| 1uy .. 6uy |])
  Util.FillBottomLeft(mem, data, 2)
  data |> shouldEqual [| 5uy; 6uy; 3uy; 4uy; 1uy; 2uy; |]

[<Test>]
let ``fill bottom left and the buffer can't hold all the data at once`` () =
  let mutable data = Array.zeroCreate 5
  use mem = new MemoryStream([| 1uy .. 5uy |])
  Util.FillBottomLeft(mem, data, 1, 2)
  data |> shouldEqual (Array.rev [| 1uy .. 5uy |])

[<Test>]
let ``fill bottom left with double pixel and the buffer can't hold all the data at once`` () =
  let mutable data = Array.zeroCreate 6
  use mem = new MemoryStream([| 1uy .. 6uy |])
  Util.FillBottomLeft(mem, data, 2, 2)
  data |> shouldEqual [| 5uy; 6uy; 3uy; 4uy; 1uy; 2uy; |]

[<Test>]
let ``fill the bottom left with padding`` () =
  let mutable data = Array.zeroCreate 6
  use mem = new MemoryStream([| 1uy .. 4uy |])
  Util.FillBottomLeft(mem, data, 2, padding = 1)
  data |> shouldEqual [| 3uy; 4uy; 0uy; 1uy; 2uy; 0uy |]

[<Test>]
let ``four is the minimum stride`` () =
  Util.Stride(width = 1, pixelDepth = 32) |> shouldEqual 4

[<Test>]
let ``stride with padding`` () =
  Util.Stride(width = 2, pixelDepth = 24) |> shouldEqual 8

[<Test>]
let ``parse targa true 24 single color`` () =
  let image = Pfim.Pfim.FromFile(Path.Combine("data", "true-24.tga"))
  let expected = [| for i in 1 .. 64 * 64 do yield! [| 255uy; 176uy; 0uy |] |]
  image.Data |> shouldEqual expected

[<Test>]
let ``parse targa true 32 single color`` () =
  let image = Pfim.Pfim.FromFile(Path.Combine("data", "true-32.tga"))
  let expected = [| for i in 1 .. 64 * 64 do yield! [| 0uy; 0uy; 127uy; 255uy |] |]
  image.Data |> shouldEqual expected

[<Test>]
let ``parse targa 32 single small run length`` () =
  let data = Array.zeroCreate 8
  let stream = [| 129uy; 2uy; 4uy; 6uy; 8uy |]
  CompressedTarga.RunLength(data, stream, 0, 0, 4)
  data |> shouldEqual [| for i in 1 .. 2 do yield! [| 2uy; 4uy; 6uy; 8uy; |] |]

[<Test>]
let ``parse targa 24 single small run length`` () =
  let data = Array.zeroCreate 6
  let stream = [| 129uy; 2uy; 4uy; 6uy |]
  CompressedTarga.RunLength(data, stream, 0, 0, 3)
  data |> shouldEqual [| for i in 1 .. 2 do yield! [| 2uy; 4uy; 6uy; |] |]

[<Test>]
let ``parse targa 24 run length`` () =
  let data = Array.zeroCreate 18
  let stream = [| 132; 2; 4; 6; 128; 8; 10; 12; |] |>  toBytes
  CompressedTarga.RunLength(data, stream, 0, 0, 3)
  let expected = seq { yield! [| for i in 1 .. 5 do yield! [| 2uy; 4uy; 6uy; |] |]
                       yield! [|0uy;0uy;0uy|] } |> Seq.toArray
  data |> shouldEqual expected

[<Test>]
let ``parse targa true 32 bit run length`` () =
  let image = Pfim.Pfim.FromFile(Path.Combine("data", "true-32-rle.tga"))
  let expected =
    seq { yield! [| for i in 1 .. 32 do yield! [| 0; 216; 255; 255 |] |]
          yield! [| for i in 1 .. 16 do yield! [| 255; 148; 0; 255 |] |]
          yield! [| for i in 1 .. 8 do yield! [| 0; 255; 76; 255 |] |]
          yield! [| for i in 1 .. 8 do yield! [| 0; 0; 255; 255; |] |] }
    |> toBytes
  image.Data |> shouldEqual expected

[<Test>]
let ``parse targa true 24 bit run length`` () =
  let image = Pfim.Pfim.FromFile(Path.Combine("data", "true-24-rle.tga"))
  let expected =
    seq { yield! [| for i in 1 .. 32 do yield! [| 0; 216; 255 |] |]
          yield! [| for i in 1 .. 16 do yield! [| 255; 148; 0 |] |]
          yield! [| for i in 1 .. 8 do yield! [| 0; 255; 76 |] |]
          yield! [| for i in 1 .. 8 do yield! [| 0; 0; 255|] |] }
    |> toBytes
  image.Data |> shouldEqual expected

[<Test>]
let ``parse targa true 32 bit mixed encoding`` () =
  let image = Pfim.Pfim.FromFile(Path.Combine("data", "true-32-mixed.tga"))
  let expected =
    seq { yield! [| for i in 1 .. 16 do yield! [| 0; 216; 255; 255 |] |]
          yield! [| 0; 0; 0; 255 |]
          yield! [| 64; 64; 64; 255 |]
          yield! [| 0; 0; 255; 255; |]
          yield! [| 0; 106; 255; 255; |]
          yield! [| 0; 216; 255; 255; |]
          yield! [| 0; 255; 182; 255; |]
          yield! [| 0; 255; 76; 255; |]
          yield! [| 33; 255; 0; 255; |]
          yield! [| 144; 255; 0; 255; |]
          yield! [| 255; 255; 0; 255; |]
          yield! [| 255; 148; 0; 255; |]
          yield! [| 255; 38; 0; 255; |]
          yield! [| 255; 0; 72; 255; |]
          yield! [| 255; 0; 178; 255; |]
          yield! [| 220; 0; 255; 255; |]
          yield! [| 110; 0; 255; 255; |]
          yield! [| for i in 1 .. 16 do yield! [| 255; 148; 0; 255 |] |]
          yield! [| for i in 1 .. 8 do yield! [| 0; 255; 76; 255 |] |]
          yield! [| for i in 1 .. 8 do yield! [| 0; 0; 255; 255 |] |] }
    |> toBytes
  image.Data |> shouldEqual expected

[<Test>]
let ``parse targa true 32 bit run length large`` () =
  let image = Pfim.Pfim.FromFile(Path.Combine("data", "true-32-rle-large.tga"))
  let expected =
    seq { yield! [| for i in 1 .. (1200 * 1200) do yield! [| 0; 51; 127; 255 |] |] }
    |> toBytes
  image.Data |> shouldEqual expected

[<Test>]
let ``parse targa top left`` () =
  let image = Pfim.Pfim.FromFile(Path.Combine("paket-files", "rgb24_top_left.tga"))
  let actual = image.Data

  actual
  |> Array.mapi (fun i v -> i, v)
  |> Seq.groupWhen (fun (i, v) -> i%3 = 0)
  |> Seq.map (Seq.map snd >> Seq.toArray)
  |> Seq.iteri (fun index arr ->
    match arr with
    | [| 0uy; 255uy; 0uy |]
    | [| 12uy; 0uy; 255uy |]
    | [| 255uy; 255uy; 255uy |] -> ()
    | [| 255uy; g; b |] when g = b -> ()
    | x -> Assert.Fail(sprintf "Unexpected color %A at: %d, %d" x (index % 64) (index / 64)))

[<Test>]
let ``parse 32 bit uncompressed dds`` () =
  let image = Pfim.Pfim.FromFile(Path.Combine("data", "32-bit-uncompressed.dds"))
  let expected = [| for i in 1 .. 64 * 64 do yield! [| 0uy; 0uy; 127uy; 255uy |] |]
  image.Data |> shouldEqual expected
  image.Height |> shouldEqual 64
  image.Width |> shouldEqual 64
  image.Format |> shouldEqual ImageFormat.Rgba32

[<Test>]
let ``parse simple dxt1`` () =
  let image = Pfim.Pfim.FromFile(Path.Combine("data", "dxt1-simple.dds"))
  let expected = [| for i in 1 .. 64 * 64 do yield! [| 0uy; 0uy; 127uy |] |]
  image.Data |> shouldEqual expected
  image.Height |> shouldEqual 64
  image.Width |> shouldEqual 64
  image.Format |> shouldEqual ImageFormat.Rgb24

[<Test>]
let ``parse simple dxt3`` () =
  let image = Pfim.Pfim.FromFile(Path.Combine("data", "dxt3-simple.dds"))
  let expected = [| for i in 1 .. 64 * 64 do yield! [| 0uy; 0uy; 128uy; 255uy |] |]
  image.Data |> shouldEqual expected
  image.Height |> shouldEqual 64
  image.Width |> shouldEqual 64
  image.Format |> shouldEqual ImageFormat.Rgba32

[<Test>]
let ``parse simple dxt5`` () =
  let image = Pfim.Pfim.FromFile(Path.Combine("data", "dxt5-simple.dds"))
  let expected = [| for i in 1 .. 64 * 64 do yield! [| 0uy; 0uy; 128uy; 255uy |] |]
  image.Data |> shouldEqual expected
  image.Height |> shouldEqual 64
  image.Width |> shouldEqual 64
  image.Format |> shouldEqual ImageFormat.Rgba32
