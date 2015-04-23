namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Pfim")>]
[<assembly: AssemblyProductAttribute("Pfim")>]
[<assembly: AssemblyDescriptionAttribute("Image file format parser")>]
[<assembly: AssemblyVersionAttribute("1.0")>]
[<assembly: AssemblyFileVersionAttribute("1.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.0"
