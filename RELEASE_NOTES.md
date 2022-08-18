### 0.11.1 - August 17th 2022

- Fix targa and dds decoding from publicly visible memory streams

### 0.11.0 - August 16th 2022

**Breaking Changes**

The `Pfim` class has been renamed to `Pfimage` to avoid namespace collision. Migration should be simple:

```diff
- using (var image = Pfim.FromFile(file))
+ using (var image = Pfimage.FromFile(file))
```

**Other Changes**

- Fix rounding errors in DXT1, DXT3, and DXT5 where the decoded channels may be inaccurate within a few degrees. See PR #98 and issue #88 for more info
- Fix image decoding on smaller buffers and chunked streams
- Pfim is now a strong named assembly
- Code samples updated to use NET 6.0

### 0.10.3 - January 1st 2022

- Add support for B5G5R5A1_UNORM encoded DDS images

### 0.10.2 - December 14th 2021

- Add initial support for b8g8r8x8 encoded DDS images

### 0.10.1 - July 8th 2021

- Add more support for TGA images with dimensions that require stride padding

### 0.10.0 - February 8th 2021

- Add support for decoding DXT1 with alpha channel. This required changing the
  image format of DXT1 images to 32 bit rgba instead of 24 bit rgb, so this is
  technically is a breaking change as code that assumed 24 bit data for DXT1
  images will need to change.
- Add a floor of 1 to dimensions of mipmap calculations

### 0.9.1 - November 20th 2019

Much thanks to @ptasev for this release:

- Add BC4 DDS support
- Add BC5S DDS support
- Add BC6H DDS support

### 0.9.0 - November 12th 2019

Much thanks to @ptasev for identifying the bugs / implementing features

- Support for BC7 DDS decoding
- Support for BC2 DDS decoding
- Support for BC5U DDS decoding
- Fixed decoding of BC3 and BC5 DDS
- Fixed blue channel for BC5 DDS images when allocator reused buffer

### 0.8.0 - September 5th 2019

Two big changes: netstandard 2.0 targeting and DDS mipmap support.

With a bump to netstandard 2.0, Pfim is able to slim down dependencies and the amount of code that is conditionally compiled. The only platforms that lose out here are platforms that are EOL or nearing EOL.

Each image file may contain several images in addition to the main image decoded. There are pre-calculated / optimized for smaller dimensions -- often called [mipmaps](https://en.wikipedia.org/wiki/Mipmap). Previously there was no way to access these images until now.

New property under `IImage`

```
MipMapOffset[] MipMaps { get; }
```

With `MipMapOffset` defined with properties:

```csharp
public int Stride { get; }
public int Width { get; }
public int Height { get; }
public int DataOffset { get; }
public int DataLen { get; }
```

These should look familiar to known `IImage` properties, the one exception being `DataOffset`. This is the offset that the mipmap data starts in `IImage.Data`. To see usage, here is a snippet for WPF to split an `IImage` into separate images by mipmaps

```csharp
private IEnumerable<BitmapSource> WpfImage(IImage image)
{
    var pinnedArray = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
    var addr = pinnedArray.AddrOfPinnedObject();
    var bsource = BitmapSource.Create(image.Width, image.Height, 96.0, 96.0, 
        PixelFormat(image), null, addr, image.DataLen, image.Stride);

    handles.Add(pinnedArray);
    yield return bsource;

    foreach (var mip in image.MipMaps)
    {
        var mipAddr = addr + mip.DataOffset;
        var mipSource = BitmapSource.Create(mip.Width, mip.Height, 96.0, 96.0,
            PixelFormat(image), null, mipAddr, mip.DataLen, mip.Stride);
        yield return mipSource;
    }
}
```

Example image:

![image](https://user-images.githubusercontent.com/2106129/63165974-a0aa0a80-bff2-11e9-9074-a7463fb286c5.png)

Only additional images are stored in `MipMaps` (the base image is excluded).

This continues the work in `0.7.0` were users should start relying on the `DataLen` property and not `Data.Length` as now multiple images share this buffer

### 0.7.0 - April 27th 2019

* Added: `Pfim.FromStream` to decode tga or dds image from a stream. Pfim will heuristically determine what image based on the header.
* Added: `IImageAllocator` that will allow one to pool intermediate byte buffers as well as the resulting data buffer. This is to reduce memory usage and GC pressure. Benchmarks have naive pooling implementations showing up to a 3x performance improvement. Set a custom allocator through `PfimConfig`
* Changed: Targa color maps are applied by default (so no need to write `IImage::ApplyColorMap` every time). If this behavior is not desired, one can set `applyColorMap` to false in `PfimConfig`.
* Changed: `IImage` implements `IDisposable` (so that the allocator can reclaim the data buffer). While disposing an image is only necessary when a custom allocator is supplied, the best practice is to still dispose an image:

```csharp
using (var image = Pfim.FromFile(file)) { }
```

* Changed: `Pfim.FromFile` no longer determines the image type by file extension. Instead it will heuristically determine the image type based on the file header.
* Changed: Exception thrown on invalid targa image types (3rd byte in the header) 

### 0.6.0 - March 28th 2019

* Added: `IImage::ApplyColorMap`, which will apply a colormap to the image, overwriting previous data and metadata like format, stride, pixel depth, etc. An example of a colormap is when an image only uses 256 colors. Instead of consuming 32 bits per pixel on disk, the image data instead will consist of 8 bit indices into the colormap located in the header of an image.
* Support Targa images orientated in the top left corner.
* Targa images encoded in the top right or bottom right corners (two extremely rare formats) fallback to bottom left corner decoding.
* Fix errors or incorrect decoding of dds images with widths and heights that aren't divisible by their block size.
* Fix `MipMapCount` misspelling in `DdsHeader`

### 0.5.2 - August 2nd 2018

* Include Mipmap data as part of `IImage::Data` for DDS images that skipped decoding
* Recognize and decode ATI2 dds images

### 0.5.1 - May 8th 2018

* Expose `BitsPerPixel` in `IImage`
* Add configuration to the decoding process via `PfimConfig`:
  * Configurable buffer size for chunk decoding
  * Allow opt-out of DDS BC decompression to allow for GPU offload.
* Optimize fast path for decoding `byte[]` data
* Latency of decoding BC DDS images decreased by 10%
* Highly experimental decoding of DX10 images.

### 0.5.0 - March 18th 2018

* Support for 24bit rgb dds images
* Support for additional 16bit dds images
  * Rgba16 (each channel is 4 bits)
  * R5g5b5a1
  * R5g6b5
  * R5g5b5
* Bug fixes for currently supported dds images
* Initial implementation for interpreting tga color maps
* Support for 16bit R5g5b5 tga images
* Support for 8bit tga images
* Fix bad calculation of tga strides

### 0.4.4 - October 31st 2017
* Fix red and blue color swap for TopLeft encoded targa images
* 20x performance improvement for TopLeft encoded targa images

### 0.4.3 - October 31st 2017
* Fix infinite loop on certain large targa and dds images

### 0.4.2 - October 10th 2017
* Release .NET Standard 1.0 version that doesn't contain File IO

### 0.4.1 - October 9th 2017
* Fix decoding of non-square uncompressed targa images
* Fix edge case decoding for compressed targa images

### 0.4.0 - September 17th 2017
* Released for netstandard 1.3
* 25% performance improvement on compressed dds images
* Bugfix in compressed targa decoder

### 0.3.1 - August 18th 2015
* Fix pixel depth calculations for compressed dds

### 0.3 - April 30 2015
* Internalized a lot of API to simplify usage
* Publish benchmarking

### 0.2 - April 29 2015
* All decoded images now derive from `IImage`

### 0.1 - April 26 2015
* Initial release
