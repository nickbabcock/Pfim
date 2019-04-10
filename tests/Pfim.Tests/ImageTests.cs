using System.IO;
using Xunit;
using static Farmhash.Sharp.Farmhash;


namespace Pfim.Tests
{
    public class ImageTests
    {
        [Theory]
        [InlineData("24-bit-uncompressed-odd.dds", 1312155254599784230, ImageFormat.Rgb24)]
        [InlineData("32-bit-uncompressed-odd.dds", 431228091138896641, ImageFormat.Rgba32)]
        [InlineData("32-bit-uncompressed.dds", 8563937048591661181, ImageFormat.Rgba32)]
        [InlineData("Antenna_Metal_0_Normal.dds", 11008763962346865605, ImageFormat.Rgb24)]
        [InlineData("CBW8.tga", 11906269729452326966, ImageFormat.Rgb8)]
        [InlineData("CCM8.tga", 11437704396291630213, ImageFormat.R5g5b5)]
        [InlineData("CTC16.tga", 11437704396291630213, ImageFormat.R5g5b5)]
        [InlineData("CYA.tga", 4966848095323908957, ImageFormat.Rgb24)]
        [InlineData("DSCN1910_24bpp_uncompressed_10_2.tga", 1527117886507111010, ImageFormat.Rgb24)]
        [InlineData("DSCN1910_24bpp_uncompressed_10_3.tga", 1527117886507111010, ImageFormat.Rgb24)]
        [InlineData("TestVolume_Noise3D.dds", 12008948602044967211, ImageFormat.Rgba32)]
        [InlineData("dds_A4R4G4B4.dds", 10024159200024540560, ImageFormat.Rgba16)]
        [InlineData("dds_A8B8G8R8.dds", 10380399580803577188, ImageFormat.Rgba32)]
        [InlineData("dds_R5G6B5.dds", 7210134307573079898, ImageFormat.R5g6b5)]
        [InlineData("dds_R8G8B8.dds", 16481500256016389047, ImageFormat.Rgb24)]
        [InlineData("dds_a1r5g5b5.dds", 12461131412352196151, ImageFormat.R5g5b5a1)]
        [InlineData("dxt1-simple.dds", 17605116077597162586, ImageFormat.Rgb24)]
        [InlineData("dxt3-simple.dds", 95660261486543378, ImageFormat.Rgba32)]
        [InlineData("dxt5-simple-1x1.dds", 17927214943315913894, ImageFormat.Rgba32)]
        [InlineData("dxt5-simple-odd.dds", 12356928648841185691, ImageFormat.Rgba32)]
        [InlineData("dxt5-simple.dds", 95660261486543378, ImageFormat.Rgba32)]
        [InlineData("flag_t32.tga", 7008764461524763534, ImageFormat.Rgba32)]
        [InlineData("large-top-left.tga", 10870824044492905452, ImageFormat.Rgb24)]
        [InlineData("marbles.tga", 10343037500488626953, ImageFormat.Rgb24)]
        [InlineData("marbles2.tga", 14623019326840994965, ImageFormat.Rgb24)]
        [InlineData("rgb24_top_left.tga", 4627684688687496028, ImageFormat.Rgb24)]
        [InlineData("rgb24_top_left_colormap.tga", 4481644023605488430, ImageFormat.Rgb24)]
        [InlineData("rgb32_top_left_rle_colormap.tga", 6150529532866763254, ImageFormat.Rgba32)]
        [InlineData("tiny-rect.tga", 15893696507487264754, ImageFormat.Rgba32)]
        [InlineData("true-24-bottom-right.tga", 1846039879287405346, ImageFormat.Rgb24)]
        [InlineData("true-24-large.tga", 17691484392545185647, ImageFormat.Rgb24)]
        [InlineData("true-24-rle.tga", 2765099695482911150, ImageFormat.Rgb24)]
        [InlineData("true-24.tga", 17372466815898695035, ImageFormat.Rgb24)]
        [InlineData("true-32-mixed.tga", 17866140955776085793, ImageFormat.Rgba32)]
        [InlineData("true-32-rle-large.tga", 724744357722860486, ImageFormat.Rgba32)]
        [InlineData("true-32-rle.tga", 2277897038200108277, ImageFormat.Rgba32)]
        [InlineData("true-32.tga", 8563937048591661181, ImageFormat.Rgba32)]
        [InlineData("wose_BC1_UNORM.DDS", 12101904599740452605, ImageFormat.Rgb24)]
        [InlineData("wose_BC1_UNORM_SRGB.DDS", 10469302966092815447, ImageFormat.Rgb24)]
        [InlineData("wose_R8G8B8A8_UNORM_SRGB.DDS", 18111997681897362439, ImageFormat.Rgba32)]
        public void TestImageProperties(string path, ulong hash, ImageFormat format)
        {
            var data = File.ReadAllBytes(Path.Combine("data", path));
            var image = Pfim.FromFile(Path.Combine("data", path));
            var image2 = Pfim.FromStream(new MemoryStream(data), new PfimConfig());
            image.ApplyColorMap();
            image2.ApplyColorMap();
            Assert.Equal(format, image.Format);
            Assert.Equal(image.Format, image2.Format);
            Assert.Equal(hash, Hash64(image.Data, image.Data.Length));
            Assert.Equal(hash, Hash64(image2.Data, image2.Data.Length));
        }
    }
}
