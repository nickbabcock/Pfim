module Pfim.Tests

open Pfim
open NUnit.Framework

[<Test>]
let ``hello returns 42`` () =
  let result = Class1.a
  printfn "%i" result
  Assert.AreEqual(42,result)
