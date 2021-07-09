<h1 align="center">
  <img src="analysis/pfim-viewer.png?raw=true">
<br/>
Pfim
</h1>

[![CI](https://github.com/nickbabcock/Pfim/actions/workflows/ci.yml/badge.svg)](https://github.com/nickbabcock/Pfim/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Pfim.svg)](https://www.nuget.org/packages/Pfim/)

**Pfim is a .NET Standard 2.0 compatible Targa (tga) and Direct Draw Surface
(dds) decoding library**

Pfim can interoperate with a multitude of environments and libraries.

- [Windows Forms (`Bitmap`)](https://github.com/nickbabcock/Pfim/tree/master/src/Pfim.Viewer.Forms)
- [WPF (`BitmapSource`)](https://github.com/nickbabcock/Pfim/tree/master/src/Pfim.Viewer)
- [MonoGame (`Texture2D`)](https://github.com/nickbabcock/Pfim/tree/master/src/Pfim.MonoGame)
- [Skia (`SKImage`)](https://github.com/nickbabcock/Pfim/tree/master/src/Pfim.Skia) (thus can work in Xamarin.Forms: iOS, android, and UWP)
- [ImageSharp (`Image`)](https://github.com/nickbabcock/Pfim/tree/master/src/Pfim.ImageSharp)
- Unity (sample pending)
- And even hooks to allow GPU decoding (sample pending)
- And more!

So no matter if you're targeting Windows, Linux, or Mac -- .NET Core or .NET Framework -- desktop, server, or mobile -- Pfim can fit.

Since Pfim emphasizes on being frontend and backend agnostic some work is entailed to coax it into a displayable form. Check out the listed samples for how to use Pfim in each scenario.

## Installation

[**Install from NuGet**](http://www.nuget.org/packages/Pfim/)

## Usage

Below is a snippet that will convert a 32bit rgba targa or direct draw surface image into a png using the .NET framework.

```csharp
using (var image = Pfim.FromFile(path))
{
    PixelFormat format;

    // Convert from Pfim's backend agnostic image format into GDI+'s image format
    switch (image.Format)
    {
        case ImageFormat.Rgba32:
            format = PixelFormat.Format32bppArgb;
            break;
        default:
            // see the sample for more details
            throw new NotImplementedException(); 
    }

    // Pin pfim's data array so that it doesn't get reaped by GC, unnecessary
    // in this snippet but useful technique if the data was going to be used in
    // control like a picture box
    var handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
    try
    {
        var data = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
        var bitmap = new Bitmap(image.Width, image.Height, image.Stride, format, data);
        bitmap.Save(Path.ChangeExtension(path, ".png"), System.Drawing.Imaging.ImageFormat.Png);
    }
    finally
    {
        handle.Free();
    }
}
```

## Benchmarks

Pfim is fast. Faster than anything benchmarked in C#.

The contestants:

- Pfim 0.7.0
- [DevIL](http://openil.sourceforge.net/) 0.2.4
- [FreeImage](http://freeimage.sourceforge.net/) 4.3.6
- [ImageMagick](https://www.imagemagick.org/script/index.php) 7.12.0
- [TargaImage](https://www.codeproject.com/Articles/31702/NET-Targa-Image-Reader) 1.0
- [ImageFormats](https://github.com/dbrant/imageformats) 1.0
- [StbSharp](https://github.com/rds1983/StbSharp) 0.7.2.38
- [TgaSharpLib](https://github.com/ALEXGREENALEX/TGASharpLib) 1.0

The task: a 120x120 image was encoded into 7 different images:  3 different
types of targa and 4 different direct draw surface images. The benchmark was
how how many times a library could decode the image.

![](analysis/decode-per-second.png?raw=true)

Takeaways:

- Pfim is hands down the fastest targa decoder. More than 10x faster than the closest competitor
- For DDS it's a little less clear except for 32bit (uncompressed) images.

Rest assured, Pfim is one of the fastest if not the fastest when it comes to
DDS decoding, but to get a clear view, let's look at the raw data.

![](analysis/median-decode.png?raw=true)

Same story, but it's now apparent that the only time Pfim doesn't come in first is when it was 1% slower for decoding dxt5 DDS images.

## Configuration

The decoding process can be customized via the `PfimConfig` parameter.

### Buffer Size

When reading from a file or stream, this is the buffer size that Pfim will read in chunks of. The default value is 32KiB (kibibytes).

### Decompress

Dictate if block encoded direct draw surface areas should be decompressed.
Default is true. The only time this should be false is if only the dds metadata
is desired or decoding will happen on the GPU.

### Allocator

Unless this option is specified, all image allocations are performed via `new
byte[]`. This can have less than desired effects on memory usage and GC
especially when many images are being decoded. To fix this issue, Pfim
introduced an `IImageAllocator` to allow a custom allocation. Below is a naive
example for buffer pooling backed by `ArrayPool`

```csharp
class PooledAllocator : IImageAllocator
{
    private readonly ArrayPool<byte> _shared = ArrayPool<byte>.Shared;

    public byte[] Rent(int size)
    {
        return _shared.Rent(size);
    }

    public void Return(byte[] data)
    {
        _shared.Return(data);
    }
}
```

### Apply Color Map

Some targa images are mapped such that 24bit pixel data is encoded as 8 bits.
Applying a color map will make the conversion from 8bit to 24bit. The default
is true.

### Target Format

Reserved for future usage to dictate decode process. For instance instead of
having a post process stage of flipping the red and blue channels, to flip them
during the decoding process so only a single pass is needed.

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
dotnet test -f netcoreapp3.1
```

Or hit "Build" in Visual Studio :smile:

Some references:

- [Targa image specification](http://www.dca.fee.unicamp.br/~martino/disciplinas/ea978/tgaffs.pdf)
- [Block compression](https://msdn.microsoft.com/en-us/library/bb694531(v=vs.85).aspx) (useful for dds)
- [DXT Compression Explained](http://www.fsdeveloper.com/wiki/index.php?title=DXT_compression_explained)
