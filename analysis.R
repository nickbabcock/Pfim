library(tidyverse)
library(readr)

df <- rbind(read_csv("TargaBenchmark-report.csv"), read_csv("DdsBenchmark-report.csv"))

payload_name <- Vectorize(function(payload) {
  switch(payload,
         "true-24-large.tga" = "24Bit Large TGA",
         "true-24.tga" = "24Bit Small TGA",
         "true-32-rle-large.tga" = "32Bit RLE Large TGA",
         "true-32-rle.tga" = "32Bit RLE Small TGA",
         "32-bit-uncompressed.dds" = "Uncompressed DDS",
         "dxt1-simple.dds" = "DXT1 DDS",
         "dxt3-simple.dds" = "DXT3 DDS",
         "dxt5-simple.dds" = "DXT5 DDS")
})

df <- df %>% mutate(Payload = payload_name(Payload), Method = decoder_name(Method)) %>%
  group_by(Payload) %>%
  mutate(Relative = min(Median) / Median)

ggplot(df, aes(Method, Relative, fill=Method)) +
  geom_bar(stat = "identity") +
  facet_grid(Payload ~ .) +
  ggtitle("Targa and Direct Draw Image Decoding Performance Comparison",
          subtitle = "with relative throughput (higher is better, best in category is 1.0)") +
  xlab("Decoder") +
  ylab("Relative Image Decoding Throughput")
