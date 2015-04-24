module Pfim.Tests

open Pfim
open NUnit.Framework
open System.IO

let shouldEqual (x : 'a) (y : 'a) = Assert.AreEqual(x, y, sprintf "Expected: %A\nActual: %A" x y)

[<Test>]
let ``hello returns 42`` () =
  let result = Class1.a
  printfn "%i" result
  Assert.AreEqual(42,result)

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

let toBytes arr = [| for i in arr -> byte(i) |]

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