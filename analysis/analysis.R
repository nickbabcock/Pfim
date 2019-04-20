library(tidyverse)
library(readr)
library(tools)
library(RColorBrewer)

# Combine the targa and dds benchmarks into a single data frame
df <- rbind(read_csv("TargaBenchmark-report.csv"), read_csv("DdsBenchmark-report.csv")) %>%
  select(Method, Payload, Median)

# Format nanoseconds to microseconds
format_units <- Vectorize(function(ns) {
  if (is.na(ns)) {
    "NA"
  } else {
    format(round(ns / 1000, 2), digits = 3)
  }
})

# For each image, group all the contestants together and compute the relative
# throughput for each one by taking their median time to decode and dividing
# it by the fastest median in the group.
throughput <- df %>%
  group_by(Payload) %>%
  mutate(Relative = min(Median) / Median) %>%
  ungroup() %>%
  complete(Method, Payload) %>%
  mutate(ImageType = ifelse(file_ext(Payload) == 'dds', "Direct Draw Surface", "Targa"))

ggplot(throughput, aes(Method, file_path_sans_ext(Payload))) +
  geom_tile(aes(fill = Relative), color = "white") +
  facet_grid(ImageType ~ ., scales = "free_y", switch = "y") +
  scale_x_discrete(position = "top") +
  scale_fill_gradient(name = "Relative", low = "white", high = "steelblue", na.value = "#D8D8D8", guide = FALSE) +
  xlab("Decoding Library") +
  ylab("Image") +
  scale_y_discrete(position = "right") +
  theme(strip.text.y = element_text(size = 12)) +
  geom_text(aes(label = format_units(Median))) +
  ggtitle("Decoding Targa and Direct Draw Surface Images on .NET",
          subtitle = paste("Median time to decode (μs) images across libraries.",
                           "Cells shaded blue relative to the fastest decoder for a given image.", sep = "\n")) +
  labs(caption = "NA: Not applicable: decoding library doesn't interpret said format")
ggsave('median-decode.png', width = 8, height = 5, dpi = 96)


ps <- throughput %>% drop_na(Median) %>% mutate(PS = 10^9 / Median)
ggplot(ps, aes(file_path_sans_ext(Payload), PS, fill=Method)) +
  geom_bar(stat="identity", position=position_dodge(), width = 0.75) +
  ylab("Decodes per Second") +
  xlab("Image") +
  scale_y_continuous(labels = scales::comma) +
  scale_x_discrete(position = "left") +
  theme(strip.text.y = element_text(size = 12)) +
  scale_fill_brewer(palette = "Dark2") +
  coord_flip() +
  facet_grid(ImageType ~ ., scales = "free_y", switch = "y") +
  ggtitle("Decoding Targa and Direct Draw Surface Images on .NET",
          subtitle = "Number of image decodes per second across libraries")
ggsave('decode-per-second.png', width = 8, height = 5, dpi = 96)
