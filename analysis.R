library(tidyverse)
library(readr)

# Combine the targa and dds benchmarks into a single data frame
df <- rbind(read_csv("TargaBenchmark-report.csv"), read_csv("DdsBenchmark-report.csv"))

# Since the payload names are not entirely display friendly, convert them
# into a more presentable form
payload_name <- Vectorize(function(payload) {
  switch(payload,
         "true-24-large.tga" = "24Bit Large TGA",
         "true-24.tga" = "24Bit Small TGA",
         "true-32-rle-large.tga" = "32Bit RLE Large TGA",
         "true-32-rle.tga" = "32Bit RLE Small TGA",
         "rgb24_top_left.tga" = "24Bit TopLeft TGA",
         "32-bit-uncompressed.dds" = "Uncompressed DDS",
         "dxt1-simple.dds" = "DXT1 DDS",
         "dxt3-simple.dds" = "DXT3 DDS",
         "dxt5-simple.dds" = "DXT5 DDS")
})

# For each image, group all the contestants together and compute the relative
# throughput for each one by taking their median time to decode and dividing
# it by the fastest median in the group.
df <- df %>% mutate(Payload = payload_name(Payload)) %>%
  group_by(Payload) %>%
  mutate(Relative = min(Median) / Median) %>%
  ungroup() %>%
  select(Method, Payload, Relative) %>%
  complete(Method, Payload)

ggplot(df, aes(Method, Payload)) +
  geom_tile(aes(fill = Relative), color = "white") +
  scale_x_discrete(position = "top") +
  scale_fill_gradient(name = "Throughput", low = "white", high = "steelblue", na.value = "#D8D8D8") +
  xlab("Decoding Library") +
  ylab("Image Decoded") +
  geom_text(aes(label = ifelse(is.na(Relative), "NA", format(round(Relative, 2), digits = 3)))) +
  ggtitle("Image Decoding with Relative Throughput",
          subtitle = "For Targa and Direct Draw Surface Images on the .NET platform. (Blue = Highest Throughput)")
