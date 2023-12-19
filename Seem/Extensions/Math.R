library(dplyr)
library(ggplot2)
library(optimr)

theme_set(theme_bw() + theme(legend.background = element_rect(fill = alpha("white", 0.5))))

# Exp2()
# fractional part regressions for IEEE-754 decompositions
exponent2 = tibble(x = seq(-0.536, 0.536, by = 0.001), exp = 2^x)
exponent2power5 = lm(exp ~ x + I(x^2) + I(x^3) + I(x^4) + I(x^5), data = exponent2)
exponent2power6 = lm(exp ~ x + I(x^2) + I(x^3) + I(x^4) + I(x^5) + I(x^6), data = exponent2)
#exponent2artanh5 = lm(exp ~ I(((x - 1)/(x + 1))) + I(((x - 1)/(x + 1))^2) + I(((x - 1)/(x + 1))^3) + I(((x - 1)/(x + 1))^4) + I(((x - 1)/(x + 1))^5), data = exponent2)
#exponent2even7 = lm(exp ~ I(x^2) + I(x^3) + I(x^5) + I(x^7), data = exponent2)
#exponent2odd7 = lm(exp ~ x + I(x^3) + I(x^5) + I(x^7), data = exponent2)
#exponent2power7 = lm(exp ~x + I(x^2) + I(x^3) + I(x^4) + I(x^5) + I(x^6) + I(x^7), data = exponent2)

ggplot() +
  geom_hline(yintercept = 1E-7 * c(-1, 1), color = "grey70", linetype = "longdash") +
  geom_path(aes(x = exponent2$x, y = exponent2power5$residuals, color = "power 5")) +
  geom_path(aes(x = exponent2$x, y = exponent2power6$residuals, color = "power 6")) +
  labs(x = "x", y = bquote("error residual, log"[2]*"(x)"), color = NULL)

# Exp()
# @jenkas https://stackoverflow.com/questions/48863719/fastest-implementation-of-exponential-function-using-avx
exponent = tibble(x = seq(-88, 88, length.out = 25000), # max range for single precision: ±88.029
                  exp = exp(x),
                  xj = 1.4426950409 * x, # 1.442695022, log2(e) = 1.442695040888963
                  integerPower = as.integer(xj),
                  integerExponent = 2^integerPower) %>%
           mutate(c1 = 0.007972914726,
                  c2 = 0.1385283768,
                  c3 = 2.885390043,
                  xj = xj - integerPower,
                  xj2 = xj * xj,
                  xj3 = xj * xj * xj,
                  a = xj + c1 * xj^3,
                  b = c3 + c2 * xj^2,
                  jenkas = (b + a) / (b - a) * integerExponent) %>%
           mutate(c1 = 0.007979176184289, # 0.007977082178740, # 0.007972141112906
                  c2 = 0.138548026377560, # 0.138541542125313, # 0.138526123085196
                  c3 = 2.885388983107529, # 2.885389361509171, # 2.885389789312397
                  a = xj + c1 * xj^3,
                  b = c3 + c2 * xj^2,
                  jenkasRefit = (b + a) / (b - a) * integerExponent)

jenkas = function(parameters)
{
  c1 = parameters[1]
  c2 = parameters[2]
  c3 = parameters[3]
  a = exponent$xj + c1 * exponent$xj3
  b = c3 + c2 * exponent$xj2
  jenkasFit = (b + a) / (b - a) * exponent$integerExponent
  error = (jenkasFit - exponent$exp) / exponent$exp
  errorSum = 1E6 * sum(error * error) # usual least squares, scale against optimpr's reltol default
  return(errorSum)
}
# single start
#jenkasRefit = optim(par = c(0.008, 0.14, 2.9), # c1, c2, c3
#                    method = "L-BFGS-B",
#                    fn = jenkas)
#sprintf("%.15f", jenkasRefit$par)
# multistart: 
# preferred: BFGS, CG
# OK: L-BFGS-B
# bad: nlm, Rcgmin, Rvmmin, hjn (very slow)
startPoints = 100
jenkasRefit = multistart(parmat = matrix(c(c1 = 0.008 + runif(n = startPoints, -0.001, 0.001), 
                                           c2 = 0.14 + runif(n = startPoints, -0.01, 0.01), 
                                           c3 = 2.9 + runif(n = startPoints, -0.1, 0.1)), 
                                         nrow = startPoints, ncol = 3),
                         fn = jenkas,
                         method = "BFGS")
sprintf("%.15f", jenkasRefit %>% slice_min(value) %>% select(p1, p2, p3, value) %>% mutate(value = 1E3 * value))
ggplot(jenkasRefit) +
  geom_histogram(aes(x = value), bins = 40) +
  scale_x_log10()

ggplot(exponent) +
  geom_path(aes(x = x, y = exp, color = "2^x")) +
  geom_path(aes(x = x, y = jenkas, color = "@jenkas")) +
  geom_path(aes(x = x, y = jenkasRefit, color = "@jenkas refit")) +
  labs(x = "power", y = "exp2(x)", color = NULL) +
  scale_y_log10()
ggplot(exponent) +
  geom_hline(yintercept = 1E-7 * c(-1, 1), color = "grey70", linetype = "longdash") +
  geom_path(aes(x = x, y = (jenkas - exp) / exp, color = "@jenkas")) +
  geom_path(aes(x = x, y = (jenkasRefit - exp) / exp, color = "@jenkas refit")) +
  labs(x = "power", y = bquote("relative error, @jenkas e"^x*" versus R e"^x), color = NULL)
