library(dplyr)
library(ggplot2)
library(patchwork)
library(tidyr)

theme_set(theme_bw() + theme(legend.background = element_rect(fill = alpha("white", 0.5))))

## neiloid heights
# Kozak A. 1988. A variable-exponent taper equation. Canadian Journal of Forest Research 18:1363-1368. https://doi.org/10.1139/x88-213
#   Table 5 for Equation 8
thplDib = crossing(dbh = seq(1, 160, by = 2), # cm
                   heightDiameterRatio = seq(20, 150, by = 10),
                   evaluationHeight = seq(0, 100, by = 0.5)) %>%
  mutate(height = 0.01 * dbh * heightDiameterRatio) %>% # m
  filter(height < 27.5 + 0.375 * dbh, height < 37.5 + 0.125 * dbh, evaluationHeight < height) %>%
  mutate(Z = evaluationHeight / height,
         X = (1 - sqrt(evaluationHeight / height)) / (1 - sqrt(0.30)),
         logDibThpl = log(1.21697) + 0.84256 * log(dbh) + log(1.00001) * dbh + 1.55322 * log(X) * Z^2 - 0.39719 * log(X) * log(Z + 0.001) + 2.11018 * log(X) * sqrt(Z) - 1.11416 * log(X) * exp(Z) + 0.09420 * log(X) * (dbh/height),
         dibThpl = exp(logDibThpl)) # cm
thplDib %>% group_by(heightDiameterRatio) %>% summarize(dbh = max(dbh), .groups = "drop") # get DBH fitting range of plots

ggplot() +
  geom_hline(yintercept = c(0.15, 1.3, 7.62 + 0.15, 12.4986 + 0.15), color = "grey70", linetype = "longdash") +
  geom_path(aes(x = 0.70 * seq(5, 159), y = pmax(-0.7 + 1/(0.02*30) + 0.01 * (0.8 + 0.075*30) * seq(5, 159), 0.15)), color = "grey70", linetype = "longdash") +
  geom_path(aes(x = dibThpl, y = evaluationHeight, color = dbh, group = 100 * dbh + heightDiameterRatio), thplDib %>% filter(dbh > 4, heightDiameterRatio == 30)) +
  coord_cartesian(xlim = c(0, 160), ylim = c(0, 25)) +
  labs(x = NULL, y = "height, m", color = "DBH, cm") +
  theme(legend.position = "none") +
ggplot() +
  geom_hline(yintercept = c(0.15, 1.3, 7.62 + 0.15, 12.4986 + 0.15), color = "grey70", linetype = "longdash") +
  geom_path(aes(x = 0.75 * seq(5, 99), y = pmax(-0.6 + 1/(0.02*50) + 0.01 * (0.8 + 0.065*50) * seq(5, 99), 0.15)), color = "grey70", linetype = "longdash") +
  geom_path(aes(x = dibThpl, y = evaluationHeight, color = dbh, group = 100 * dbh + heightDiameterRatio), thplDib %>% filter(dbh > 4, heightDiameterRatio == 50)) +
  coord_cartesian(xlim = c(0, 160), ylim = c(0, 25)) +
  labs(x = NULL, y = NULL, color = "DBH, cm") +
  theme(legend.position = "none") +
ggplot() +
  geom_hline(yintercept = c(0.15, 1.3, 7.62 + 0.15, 12.4986 + 0.15), color = "grey70", linetype = "longdash") +
  geom_path(aes(x = 0.86 * seq(5, 55), y = pmax(-0.5 + 1/(0.02*80) + 0.01 * (0.8 + 0.055*80) * seq(5, 55), 0.15)), color = "grey70", linetype = "longdash") +
  geom_path(aes(x = dibThpl, y = evaluationHeight, color = dbh, group = 100 * dbh + heightDiameterRatio), thplDib %>% filter(dbh > 4, heightDiameterRatio == 80)) +
  coord_cartesian(xlim = c(0, 160), ylim = c(0, 25)) +
  labs(x = "western redcedar dib, cm", y = "height, m", color = "DBH, cm") +
  theme(legend.position = "none") +
ggplot() +
  geom_hline(yintercept = c(0.15, 1.3, 7.62 + 0.15, 12.4986 + 0.15), color = "grey70", linetype = "longdash") +
  geom_path(aes(x = 1.0 * seq(5, 23), y = pmax(-0.4 + 1/(0.02*150) + 0.01 * (0.8 + 0.045*150) * seq(5, 23), 0.15)), color = "grey70", linetype = "longdash") +
  geom_path(aes(x = dibThpl, y = evaluationHeight, color = dbh, group = 100 * dbh + heightDiameterRatio), thplDib %>% filter(dbh > 4, heightDiameterRatio == 150)) +
  coord_cartesian(xlim = c(0, 160), ylim = c(0, 25)) +
  labs(x = "western redcedar dib, cm", y = NULL, color = "DBH, cm") +
  theme(legend.justification = c(1, 1), legend.position = c(0.98, 0.98)) +
plot_layout(nrow = 2, ncol = 2, widths = c(1.1, 1), heights = c(1, 1.1))

thplNeioid = tibble(heightDiameterRatio = c(30, 50, 80, 150),
                     intercept = c(-0.7, -0.6, -0.5, -0.4) + 1/(0.02 * heightDiameterRatio),
                     slope = 0.8 + c(0.075, 0.065, 0.055, 0.045) * heightDiameterRatio)
ggplot(thplNeioid) +
  geom_path(aes(x = heightDiameterRatio, y = -0.35 + 1/(0.026*heightDiameterRatio), color = "intercept")) +
  geom_path(aes(x = heightDiameterRatio, y = 2.0 + 0.038*heightDiameterRatio, color = "slope")) +
  geom_point(aes(x = heightDiameterRatio, y = intercept, color = "intercept")) +
  geom_point(aes(x = heightDiameterRatio, y = slope, color = "slope")) +
  labs(x = "height-diameter ratio", y = "western redcedar coefficient", color = NULL) +
  theme(legend.justification = c(0, 1), legend.position = c(0.02, 0.98))
