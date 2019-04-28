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
