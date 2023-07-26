library(dplyr)
library(ggplot2)
library(patchwork)
library(tidyr)

theme_set(theme_bw() + theme(legend.background = element_rect(fill = alpha("white", 0.5))))


## neiloid heights
# Poudel K, Temesgen H, Gray AN. 2018. Estimating upper stem diameters and volume of Douglas-fir and Western hemlock
#   trees in the Pacific northwest. Forest Ecosystems 5:16. https://doi.org/10.1186/s40663-018-0134-2
poudelDib = crossing(dbh = seq(1, 120), # cm
                     heightDiameterRatio = seq(20, 150, by = 10),
                     evaluationHeight = c(0.15, 0.30, 0.65, 1, 1.37, seq(2, 75))) %>%
  mutate(height = 0.01 * dbh * heightDiameterRatio) %>% # m
  filter(height < 27.5 + 0.375 * dbh, height < 30 + 0.25 * dbh, evaluationHeight < height) %>%
  mutate(t = evaluationHeight / height,
         k = 1.3 / height,
         oneMinusCubeRootT = 1 - t^(1/3),
         tkRatio = oneMinusCubeRootT / (1 - k^(1/3)),
         dibPsme = 1.04208 * dbh^0.99771 * height^-0.03111 * tkRatio^(0.53788 * t^4 - 1.01291 / exp(dbh / height) + 0.56813 * tkRatio^0.1 + 4.96019 / dbh + 0.04124 * height^oneMinusCubeRootT - 0.34417 * tkRatio), # cm
         dibTshe = 1.05981 * dbh^0.99433 * height^-0.01684 * tkRatio^(0.64632 * t^4 - 1.56599 / exp(dbh / height) + 0.74293 * tkRatio^0.1 + 4.75618 / dbh + 0.0389 * height^oneMinusCubeRootT - 0.19425 * tkRatio)) # cm
poudelDib %>% group_by(heightDiameterRatio) %>% summarize(dbh = max(dbh), .groups = "drop") # get DBH fitting range of plots

# Douglas-fir neiloid heights
ggplot() +
  geom_hline(yintercept = c(0.15, 1.3, 7.62 + 0.15, 12.4986 + 0.15), color = "grey70", linetype = "longdash") +
  geom_path(aes(x = 0.83 * seq(5, 120), y = pmax(-0.7 + 1/(0.02*30) + 0.01 * (0.8 + 0.045*30) * seq(5, 120), 0.15)), color = "grey70", linetype = "longdash") + # neiloid line
  geom_path(aes(x = dibPsme, y = evaluationHeight, color = dbh, group = 100 * dbh + heightDiameterRatio), poudelDib %>% filter(dbh > 4, heightDiameterRatio == 30)) +
  annotate("text", x = 120, y = 50, label = "height:diameter = 30", hjust = 1, size = 3) +
  coord_cartesian(xlim = c(0, 120), ylim = c(0, 50)) +
  labs(x = NULL, y = "height, m", color = "DBH, cm") +
  theme(legend.position = "none") +
ggplot() +
  geom_hline(yintercept = c(0.15, 1.3, 7.62 + 0.15, 12.4986 + 0.15), color = "grey70", linetype = "longdash") +
  geom_path(aes(x = 0.82 * seq(5, 119), y = pmax(-0.7 + 1/(0.02*50) + 0.01 * (0.8 + 0.045*50) * seq(5, 119), 0.15)), color = "grey70", linetype = "longdash") + # neiloid line
  geom_path(aes(x = dibPsme, y = evaluationHeight, color = dbh, group = 100 * dbh + heightDiameterRatio), poudelDib %>% filter(dbh > 4, heightDiameterRatio == 50)) +
  annotate("text", x = 120, y = 50, label = "height:diameter = 50", hjust = 1, size = 3) +
  coord_cartesian(xlim = c(0, 120), ylim = c(0, 50)) +
  labs(x = NULL, y = NULL, color = "DBH, cm") +
  theme(legend.position = "none") +
ggplot() +
  geom_hline(yintercept = c(0.15, 1.3, 7.62 + 0.15, 12.4986 + 0.15), color = "grey70", linetype = "longdash") +
  geom_path(aes(x = 0.9 * seq(5, 54), y = pmax(-0.7 + 1/(0.02*80) + 0.01 * (0.8 + 0.045*80) * seq(5, 54), 0.15)), color = "grey70", linetype = "longdash") + # neiloid line
  geom_path(aes(x = dibPsme, y = evaluationHeight, color = dbh, group = 100 * dbh + heightDiameterRatio), poudelDib %>% filter(dbh > 4, heightDiameterRatio == 80)) +
  annotate("text", x = 120, y = 50, label = "height:diameter = 80", hjust = 1, size = 3) +
  coord_cartesian(xlim = c(0, 120), ylim = c(0, 50)) +
  labs(x = "Douglas-fir dib, cm", y = "height, m", color = "DBH, cm") +
  theme(legend.position = "none") +
ggplot() +
  geom_hline(yintercept = c(0.15, 1.3, 7.62 + 0.15, 12.4986 + 0.15), color = "grey70", linetype = "longdash") +
  geom_path(aes(x = 1.0 * seq(5, 23), y = pmax(-0.7 + 1/(0.02*150) + 0.01 * (0.8 + 0.045*150) * seq(5, 23), 0.15)), color = "grey70", linetype = "longdash") + # neiloid line
  geom_path(aes(x = dibPsme, y = evaluationHeight, color = dbh, group = 100 * dbh + heightDiameterRatio), poudelDib %>% filter(dbh > 4, heightDiameterRatio == 150)) +
  annotate("text", x = 120, y = 50, label = "height:diameter = 150", hjust = 1, size = 3) +
  coord_cartesian(xlim = c(0, 120), ylim = c(0, 50)) +
  labs(x = "Douglas-fir dib, cm", y = NULL, color = "DBH, cm") +
  theme(legend.justification = c(1, 1), legend.position = c(0.98, 0.98)) +
plot_annotation(theme = theme(plot.margin = margin())) +
plot_layout(nrow = 2, ncol = 2, widths = c(1.1, 1), heights = c(1, 1.1))

psmeNeiloid = tibble(heightDiameterRatio = c(30, 50, 80, 150),
                     intercept = c(0.9, 0.4, -0.1, -0.3),
                     slope = c(2.25, 2.6, 4, 8))
ggplot(psmeNeiloid) +
  geom_path(aes(x = heightDiameterRatio, y = -0.7 + 1/(0.02*heightDiameterRatio), color = "intercept")) +
  geom_path(aes(x = heightDiameterRatio, y = 0.8 + 0.045*heightDiameterRatio, color = "slope")) +
  geom_point(aes(x = heightDiameterRatio, y = intercept, color = "intercept")) +
  geom_point(aes(x = heightDiameterRatio, y = slope, color = "slope")) +
  labs(x = "height-diameter ratio", y = "Douglas-fir coefficient", color = NULL)
  theme(legend.justification = c(0, 1), legend.position = c(0.02, 0.98))

# western hemlock neiloid heights
ggplot() +
  geom_hline(yintercept = c(0.15, 1.3, 7.62 + 0.15, 12.4986 + 0.15), color = "grey70", linetype = "longdash") +
  geom_path(aes(x = 0.81 * seq(5, 120), y = pmax(0.4 - 1/(0.035*30) + 0.01 * (3.25 + 0.025*30) * seq(5, 120), 0.15)), color = "grey70", linetype = "longdash") + # neiloid line
  #geom_path(aes(x = 0.81 * seq(5, 120), y = pmax(-0.6 + 0.01 * (4.0) * seq(5, 120), 0.15)), color = "grey70", linetype = "longdash") +
  geom_path(aes(x = dibTshe, y = evaluationHeight, color = dbh, group = 100 * dbh + heightDiameterRatio), poudelDib %>% filter(dbh > 4, heightDiameterRatio == 30)) +
  annotate("text", x = 120, y = 50, label = "height:diameter = 30", hjust = 1, size = 3) +
  coord_cartesian(xlim = c(0, 120), ylim = c(0, 50)) +
  labs(x = NULL, y = "height, m", color = "DBH, cm") +
  theme(legend.position = "none") +
ggplot() +
  geom_hline(yintercept = c(0.15, 1.3, 7.62 + 0.15, 12.4986 + 0.15), color = "grey70", linetype = "longdash") +
  geom_path(aes(x = 0.80 * seq(5, 119), y = pmax(0.4 - 1/(0.035*50) + 0.01 * (3.25 + 0.025*50) * seq(5, 119), 0.15)), color = "grey70", linetype = "longdash") + # neiloid line
  #geom_path(aes(x = 0.80 * seq(5, 119), y = pmax(-0.4 + 0.01 * (4.5) * seq(5, 119), 0.15)), color = "grey70", linetype = "longdash") +
  geom_path(aes(x = dibTshe, y = evaluationHeight, color = dbh, group = 100 * dbh + heightDiameterRatio), poudelDib %>% filter(dbh > 4, heightDiameterRatio == 50)) +
  annotate("text", x = 120, y = 50, label = "height:diameter = 50", hjust = 1, size = 3) +
  coord_cartesian(xlim = c(0, 120), ylim = c(0, 50)) +
  labs(x = NULL, y = NULL, color = "DBH, cm") +
  theme(legend.position = "none") +
ggplot() +
  geom_hline(yintercept = c(0.15, 1.3, 7.62 + 0.15, 12.4986 + 0.15), color = "grey70", linetype = "longdash") +
  geom_path(aes(x = 0.92 * seq(5, 54), y = pmax(0.4 - 1/(0.035*80) + 0.01 * (3.25 + 0.025*80) * seq(5, 54), 0.15)), color = "grey70", linetype = "longdash") + # neiloid line
  #geom_path(aes(x = 0.92 * seq(5, 54), y = pmax(0.1 + 0.01 * (5.5) * seq(5, 54), 0.15)), color = "grey70", linetype = "longdash") +
  geom_path(aes(x = dibTshe, y = evaluationHeight, color = dbh, group = 100 * dbh + heightDiameterRatio), poudelDib %>% filter(dbh > 4, heightDiameterRatio == 80)) +
  annotate("text", x = 120, y = 50, label = "height:diameter = 80", hjust = 1, size = 3) +
  coord_cartesian(xlim = c(0, 120), ylim = c(0, 50)) +
  labs(x = "western hemlock dib, cm", y = "height, m", color = "DBH, cm") +
  theme(legend.position = "none") +
ggplot() +
  geom_hline(yintercept = c(0.15, 1.3, 7.62 + 0.15, 12.4986 + 0.15), color = "grey70", linetype = "longdash") +
  geom_path(aes(x = 1.0 * seq(5, 23), y = pmax(0.4 - 1/(0.035*150) + 0.01 * (3.25 + 0.025*150) * seq(5, 23), 0.15)), color = "grey70", linetype = "longdash") + # neiloid line
  #geom_path(aes(x = 1.0 * seq(5, 23), y = pmax(0.2 + 0.01 * (7.0) * seq(5, 23), 0.15)), color = "grey70", linetype = "longdash") +
  geom_path(aes(x = dibTshe, y = evaluationHeight, color = dbh, group = 100 * dbh + heightDiameterRatio), poudelDib %>% filter(dbh > 4, heightDiameterRatio == 150)) +
  annotate("text", x = 120, y = 50, label = "height:diameter = 150", hjust = 1, size = 3) +
  coord_cartesian(xlim = c(0, 120), ylim = c(0, 50)) +
  labs(x = "western hemlock dib, cm", y = NULL, color = "DBH, cm") +
  theme(legend.justification = c(1, 1), legend.position = c(0.98, 0.98)) +
plot_layout(nrow = 2, ncol = 2, widths = c(1.1, 1), heights = c(1, 1.1))

tsheNeiloid = tibble(heightDiameterRatio = c(30, 50, 80, 150),
                     intercept = c(-0.6, -0.4, 0.1, 0.2),
                     slope = c(4.0, 4.5, 5.5, 7.0))
ggplot(tsheNeiloid) +
  geom_path(aes(x = heightDiameterRatio, y = 0.4 - 1/(0.035*heightDiameterRatio), color = "intercept")) +
  geom_path(aes(x = heightDiameterRatio, y = 3.25 + 0.025*heightDiameterRatio, color = "slope")) +
  geom_point(aes(x = heightDiameterRatio, y = intercept, color = "intercept")) +
  geom_point(aes(x = heightDiameterRatio, y = slope, color = "slope")) +
  labs(x = "height-diameter ratio", y = "hemlock coefficient", color = NULL) +
  theme(legend.justification = c(0, 1), legend.position = c(0.02, 0.98))
