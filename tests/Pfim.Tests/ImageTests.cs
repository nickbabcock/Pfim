using System.IO;
using System.Linq;
using Xunit;
using static Farmhash.Sharp.Farmhash;


namespace Pfim.Tests
{
    public class ImageTests
    {
        [Theory]
        [InlineData("24-bit-odd.tga", 14709551298911859837, ImageFormat.Rgb24)]
        [InlineData("24-bit-uncompressed-odd.dds", 1312155254599784230, ImageFormat.Rgb24)]
        [InlineData("32-bit-uncompressed-odd.dds", 431228091138896641, ImageFormat.Rgba32)]
        [InlineData("32-bit-uncompressed.dds", 8563937048591661181, ImageFormat.Rgba32)]
        [InlineData("Antenna_Metal_0_Normal.dds", 11008763962346865605, ImageFormat.Rgb24)]
        [InlineData("b8g8r8x8.dds", 14954708280773054506, ImageFormat.Rgba32)]
        [InlineData("b5g5r5a1.dds", 10104226901531980508, ImageFormat.R5g5b5a1)]
        [InlineData("CBW8.tga", 11906269729452326966, ImageFormat.Rgb8)]
        [InlineData("CCM8.tga", 11437704396291630213, ImageFormat.R5g5b5)]
        [InlineData("CTC16.tga", 11437704396291630213, ImageFormat.R5g5b5)]
        [InlineData("CYA.tga", 4966848095323908957, ImageFormat.Rgb24)]
        [InlineData("DSCN1910_24bpp_uncompressed_10_2.tga", 1527117886507111010, ImageFormat.Rgb24)]
        [InlineData("DSCN1910_24bpp_uncompressed_10_3.tga", 1527117886507111010, ImageFormat.Rgb24)]
        [InlineData("TestVolume_Noise3D.dds", 12008948602044967211, ImageFormat.Rgba32)]
        [InlineData("colormap-odd.tga", 15577927903454497512, ImageFormat.Rgb24)]
        [InlineData("dds_A4R4G4B4.dds", 10024159200024540560, ImageFormat.Rgba16)]
        [InlineData("dds_A8B8G8R8.dds", 10380399580803577188, ImageFormat.Rgba32)]
        [InlineData("dds_R5G6B5.dds", 7210134307573079898, ImageFormat.R5g6b5)]
        [InlineData("dds_R8G8B8.dds", 16481500256016389047, ImageFormat.Rgb24)]
        [InlineData("dds_a1r5g5b5.dds", 12461131412352196151, ImageFormat.R5g5b5a1)]
        [InlineData("dxt1-simple.dds", 8563937048591661181, ImageFormat.Rgba32)]
        [InlineData("dxt1-alpha.dds", 6057963782908520357, ImageFormat.Rgba32)]
        [InlineData("dxt3-simple.dds", 95660261486543378, ImageFormat.Rgba32)]
        [InlineData("dxt5-simple-1x1.dds", 17927214943315913894, ImageFormat.Rgba32)]
        [InlineData("dxt5-simple-odd.dds", 12356928648841185691, ImageFormat.Rgba32)]
        [InlineData("dxt5-simple.dds", 95660261486543378, ImageFormat.Rgba32)]
        [InlineData("bc2-simple-srgb.dds", 6636373717799097053, ImageFormat.Rgba32)]
        [InlineData("bc3-simple-srgb.dds", 6636373717799097053, ImageFormat.Rgba32)]
        [InlineData("bc4-simple.dds", 3506296397725802394, ImageFormat.Rgb8)]
        [InlineData("bc5-simple.dds", 13708866582685238694, ImageFormat.Rgb24)]
        [InlineData("bc5-simple-snorm.dds", 71597488204904769, ImageFormat.Rgb24)]
        [InlineData("bc6h-simple.dds", 3916824648785599771, ImageFormat.Rgba32)]
        [InlineData("bc7-simple.dds", 6448059429400395812, ImageFormat.Rgba32)]
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
        [InlineData("wose_BC1_UNORM.DDS", 12222833840891720957, ImageFormat.Rgba32)]
        [InlineData("wose_BC1_UNORM_SRGB.DDS", 12371627260059712210, ImageFormat.Rgba32)]
        [InlineData("wose_R8G8B8A8_UNORM_SRGB.DDS", 18111997681897362439, ImageFormat.Rgba32)]
        [InlineData("bench\\32bit.dds", 15068097721520544352, ImageFormat.Rgba32)]
        [InlineData("bench\\dxt3.dds", 15068097721520544352, ImageFormat.Rgba32)]
        [InlineData("bench\\dxt5.dds", 15068097721520544352, ImageFormat.Rgba32)]
        [InlineData("bench\\32bit.tga", 15068097721520544352, ImageFormat.Rgba32)]
        [InlineData("bench\\32bit-rle.tga", 15068097721520544352, ImageFormat.Rgba32)]
        [InlineData("bench\\24bit-rle.tga", 2999539589530288153, ImageFormat.Rgb24)]
        [InlineData("bench\\dxt1.dds", 15068097721520544352, ImageFormat.Rgba32)]
        public void TestImageProperties(string allPath, ulong hash, ImageFormat format)
        {
            var path = Path.Combine("data", Path.Combine(allPath.Split('\\')));
            var data = File.ReadAllBytes(path);
            var image = Pfim.FromFile(path);
            var image2 = Pfim.FromStream(new MemoryStream(data), new PfimConfig());
            Assert.NotEqual(0, image.DataLen);
            Assert.NotEqual(0, image2.DataLen);

            var allocator = new PfimAllocator();
            Assert.Equal(0, allocator.Rented);

            using (var image3 = Pfim.FromStream(new MemoryStream(data), new PfimConfig(allocator: allocator)))
            {
                Assert.Equal(format, image.Format);
                Assert.Equal(image.Format, image2.Format);
                Assert.Equal(hash, Hash64(image.Data, image.DataLen));
                Assert.Equal(hash, Hash64(image2.Data, image2.DataLen));
                Assert.Equal(hash, Hash64(image3.Data, image3.DataLen));

                var mipMapSuffix = image.MipMaps.Sum(x => x.DataLen);

                Assert.Equal(image.Data.Length - mipMapSuffix, image.DataLen);
                Assert.Equal(image.Data.Length, image2.Data.Length);
                Assert.Equal(image3.DataLen, image.Data.Length - mipMapSuffix);
                Assert.NotEqual(0, image.DataLen);
                Assert.NotEqual(0, allocator.Rented);
            }

            Assert.Equal(0, allocator.Rented);
        }

        [Theory]
        [InlineData("Antenna_Metal_0_Normal.dds", 4050857792466399628)]
        [InlineData("wose_BC1_UNORM.DDS", 8862915425226549962)]
        [InlineData("wose_R8G8B8A8_UNORM_SRGB.DDS", 15265978687041743669)]
        public void TestMipMapProperties(string allPath, ulong hash)
        {
            var path = Path.Combine("data", Path.Combine(allPath.Split('\\')));
            var data = File.ReadAllBytes(path);
            var image = Pfim.FromFile(path);
            var image2 = Dds.Create(data, new PfimConfig());
            Assert.Equal(image.MipMaps, image2.MipMaps);

            var mipMapLengths = image.MipMaps.Sum(x => x.DataLen);
            var hash1 = Hash64(image.Data, image.DataLen + mipMapLengths);
            Assert.Equal(hash1, Hash64(image2.Data, image2.DataLen + mipMapLengths));
            Assert.Equal(hash, hash1);
        }
    }
}
