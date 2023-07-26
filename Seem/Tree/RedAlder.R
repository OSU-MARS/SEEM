library(dplyr)
library(ggplot2)
library(patchwork)
library(tidyr)

theme_set(theme_bw() + theme(legend.background = element_rect(fill = alpha("white", 0.5))))

## neiloid heights
# Hibbs D, Bluhm A, Garber S. 2007. Stem Taper and Volume of Managed Red Alder. Western Journal of Applied Forestry 22(1): 61–66.
#  https://doi.org/10.1093/wjaf/22.1.61
alruDib = crossing(dbh = seq(1, 100, by = 2), # cm
                   heightDiameterRatio = seq(20, 150, by = 10),
                   evaluationHeight = seq(0, 70, by = 0.5)) %>%
  mutate(height = 0.01 * dbh * heightDiameterRatio) %>% # m
  filter(height < 27.5 + 0.375 * dbh, height < 37.5 + 0.125 * dbh, evaluationHeight < height) %>%
  mutate(heightInFeet = 3.28084 * height,
         dbhInInches = 0.393701 * dbh,
         Z = evaluationHeight / height,
         X = (1 - sqrt(Z)) / (1 - sqrt(4.5 / height)),
         dibAlru = 2.54 * 0.8995 * dbhInInches^1.0205 * X^(0.2631 * (1.364409 * dbhInInches^(1/3) * exp(-18.8990 * Z) + exp(4.2549 * (dbhInInches / heightInFeet)^0.6221 * Z)))) # inches to cm
alruDib %>% group_by(heightDiameterRatio) %>% summarize(dbh = max(dbh), .groups = "drop") # get DBH fitting range of plots

ggplot() +
  geom_hline(yintercept = c(0.15, 1.3, 7.62 + 0.15, 12.4986 + 0.15), color = "grey70", linetype = "longdash") +
  geom_path(aes(x = 0.70 * seq(5, 135), y = pmax(-0.5 + 1/(0.02*30) + 0.01 * (0.8 + 0.065*30) * seq(5, 135), 0.15)), color = "grey70", linetype = "longdash") + # neiloid line
  geom_path(aes(x = dibAlru, y = evaluationHeight, color = dbh, group = 100 * dbh + heightDiameterRatio), alruDib %>% filter(dbh > 4, heightDiameterRatio == 30)) +
  annotate("text", x = 125, y = 50, label = "height:diameter = 30", hjust = 1, size = 3) +
  coord_cartesian(xlim = c(0, 125), ylim = c(0, 50)) +
  labs(x = NULL, y = "height, m", color = "DBH, cm") +
  theme(legend.position = "none") +
ggplot() +
  geom_hline(yintercept = c(0.15, 1.3, 7.62 + 0.15, 12.4986 + 0.15), color = "grey70", linetype = "longdash") +
  geom_path(aes(x = 0.75 * seq(5, 125), y = pmax(-0.5 + 1/(0.02*50) + 0.01 * (0.8 + 0.065*50) * seq(5, 125), 0.15)), color = "grey70", linetype = "longdash") + # neiloid line
  geom_path(aes(x = dibAlru, y = evaluationHeight, color = dbh, group = 100 * dbh + heightDiameterRatio), alruDib %>% filter(dbh > 4, heightDiameterRatio == 50)) +
  annotate("text", x = 125, y = 50, label = "height:diameter = 50", hjust = 1, size = 3) +
  coord_cartesian(xlim = c(0, 125), ylim = c(0, 50)) +
  labs(x = NULL, y = NULL, color = "DBH, cm") +
  theme(legend.position = "none") +
ggplot() +
  geom_hline(yintercept = c(0.15, 1.3, 7.62 + 0.15, 12.4986 + 0.15), color = "grey70", linetype = "longdash") +
  geom_path(aes(x = 0.86 * seq(5, 63), y = pmax(-0.4 + 1/(0.02*80) + 0.01 * (0.8 + 0.075*80) * seq(5, 63), 0.15)), color = "grey70", linetype = "longdash") + # neiloid line
  geom_path(aes(x = dibAlru, y = evaluationHeight, color = dbh, group = 100 * dbh + heightDiameterRatio), alruDib %>% filter(dbh > 4, heightDiameterRatio == 80)) +
  annotate("text", x = 125, y = 50, label = "height:diameter = 80", hjust = 1, size = 3) +
  coord_cartesian(xlim = c(0, 125), ylim = c(0, 50)) +
  labs(x = "red alder dib, cm", y = "height, m", color = "DBH, cm") +
  theme(legend.position = "none") +
ggplot() +
  geom_hline(yintercept = c(0.15, 1.3, 7.62 + 0.15, 12.4986 + 0.15), color = "grey70", linetype = "longdash") +
  geom_path(aes(x = 1.0 * seq(5, 23), y = pmax(-0.3 + 1/(0.02*150) + 0.01 * (0.8 + 0.080*150) * seq(5, 23), 0.15)), color = "grey70", linetype = "longdash") + # neiloid line
  geom_path(aes(x = dibAlru, y = evaluationHeight, color = dbh, group = 100 * dbh + heightDiameterRatio), alruDib %>% filter(dbh > 4, heightDiameterRatio == 150)) +
  annotate("text", x = 125, y = 50, label = "height:diameter = 150", hjust = 1, size = 3) +
  coord_cartesian(xlim = c(0, 125), ylim = c(0, 50)) +
  labs(x = "red alder dib, cm", y = NULL, color = "DBH, cm") +
  theme(legend.justification = c(1, 1), legend.position = c(0.98, 0.92)) +
plot_annotation(theme = theme(plot.margin = margin())) +
plot_layout(nrow = 2, ncol = 2, widths = c(1.1, 1), heights = c(1, 1.1))

alruNeioid = tibble(heightDiameterRatio = c(30, 50, 80, 150),
                    intercept = c(-0.5, -0.5, -0.4, -0.3) + 1/(0.02 * heightDiameterRatio), # coefficients from neloid lines above
                    slope = 0.8 + c(0.065, 0.065, 0.075, 0.080) * heightDiameterRatio) # coefficients from neloid lines above
ggplot(alruNeioid) +
  geom_path(aes(x = heightDiameterRatio, y = -0.22 + 1/(0.025*heightDiameterRatio), color = "intercept")) +
  geom_path(aes(x = heightDiameterRatio, y = 0.1 + 0.084*heightDiameterRatio, color = "slope")) +
  geom_point(aes(x = heightDiameterRatio, y = intercept, color = "intercept")) +
  geom_point(aes(x = heightDiameterRatio, y = slope, color = "slope")) +
  labs(x = "height-diameter ratio", y = "red alder coefficient", color = NULL) +
  theme(legend.justification = c(0, 1), legend.position = c(0.02, 0.98))
