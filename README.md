[![Build status](https://ci.appveyor.com/api/projects/status/bmc00ghxk0cvv0wi/branch/master?svg=true)](https://ci.appveyor.com/project/nickbabcock/pfim/branch/master)
[![Build status](https://travis-ci.org/nickbabcock/Pfim.svg?branch=master)](https://travis-ci.org/nickbabcock/Pfim)

# Pfim

Pfim is a .NET Standard 1.0 compatible Targa (tga) and Direct Draw Surface
(dds) decoding library with an emphasis on speed and ease of use. Pfim can be
used on your linux server, Windows Form, or WPF app!

See the [main site](https://nickbabcock.github.io/Pfim/) for usage,
benchmarks against other libraries, and integrations.

## Developer Resources

Building the library is as easy as

```
dotnet test -f netcoreapp2.0 tests/Pfim.Tests/Pfim.Tests.csproj
```

Or hit "Build" in Visual Studio :smile:

- [Targa image specification](http://www.dca.fee.unicamp.br/~martino/disciplinas/ea978/tgaffs.pdf)
- [Block compression](https://msdn.microsoft.com/en-us/library/bb694531(v=vs.85).aspx) (useful for dds)
- [DXT Compression Explained](http://www.fsdeveloper.com/wiki/index.php?title=DXT_compression_explained)
