(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../bin"

(**

# Introducing Pfim

Pfim is an incredibly simple and fast image decoding library with an emphasis
on being backend and frontend agnostic. This means that you can add Pfim to
your server, Windows Form, or WPF app! The downside is that you may have to
put in a little work to get into a useable form, but as you'll see it is not
that hard!

    [lang=csharp]
    // Load image from file path
    IImage image = Pfim.FromFile(@"C:\image.tga");

That's it! If you happen to be dealing with streams, you may opt to use more
specific APIs to load an image

    [lang=csharp]
    // Obtain a stream of data somehow
    var stream = new MemoryStream();

    // Creates a direct draw surface image
    IImage image = Dds.Create(stream);

    // Creates a targa image
    IImage image2 = Targa.Create(stream);

## Integrations

### WPF

Displaying images decoded by Pfim in WPF is simple. The hardest part is
knowing how to translate Pfim's image format to WPF `PixelFormats`. The
example shows such conversion.

    [lang=csharp]
    IImage image = Pfim.FromFile(@"C:\image.tga");

    PixelFormats format;
    switch (image.Format)
    {
        case ImageFormat.Rgb24:
            format = PixelFormats.Bgr24;
            break;

        case ImageFormat.Rgba32:
            format = PixelFormats.Bgr32;
            break;

        default:
            throw new ApplicationException("Format not recognized");
    }

    // Create a WPF ImageSource and then set an Image to our variable.
    // Make sure you notify property changes as appropriate ;)
    ImageSource source = BitmapSource.Create(image.Width, image.Height,
        96.0, 96.0, format, null, image.Data, image.Stride);

### Others coming

*)
