[![Build Status](https://dev.azure.com/nbabcock19/nbabcock19/_apis/build/status/nickbabcock.Pfim?branchName=master)](https://dev.azure.com/nbabcock19/nbabcock19/_build/latest?definitionId=3&branchName=master)

# Pfim

Pfim is a .NET Standard 1.0 compatible Targa (tga) and Direct Draw Surface
(dds) decoding library with an emphasis on speed and ease of use. Pfim can be
used on .NET core, .NET 4.6, or Mono, so there is almost no place Pfim can't be
deployed.

See the [main site](https://nickbabcock.github.io/Pfim/) for usage,
benchmarks against other libraries, and integrations.

## Contributing

All contributions are welcome. Here is a quick guideline:

- Does your image fail to parse or look incorrect? File an issue with the image attached.
- Want Pfim to support more image codecs? Raise an issue to let everyone know you're working on it and then get to work!
- Have a performance improvement for Pfim? Excellent, run the before and after benchmarks!

```
dotnet build -c Release -f net461  .\src\Pfim.Benchmarks\Pfim.Benchmarks.csproj
cd src\Pfim.Benchmarks\bin\Release\net461
.\Pfim.Benchmarks.exe --filter *.Pfim
```

- Know a library to include in the benchmarks? If it is NuGet installable / easily integratable, please raise an issue or pull request! It must run on .NET 4.6.

## Developer Resources

Building the library is as easy as

```
dotnet test -f netcoreapp2.0 tests/Pfim.Tests/Pfim.Tests.csproj
```

Or hit "Build" in Visual Studio :smile:

- [Targa image specification](http://www.dca.fee.unicamp.br/~martino/disciplinas/ea978/tgaffs.pdf)
- [Block compression](https://msdn.microsoft.com/en-us/library/bb694531(v=vs.85).aspx) (useful for dds)
- [DXT Compression Explained](http://www.fsdeveloper.com/wiki/index.php?title=DXT_compression_explained)
