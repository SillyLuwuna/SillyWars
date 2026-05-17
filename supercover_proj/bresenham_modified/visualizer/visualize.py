import matplotlib.pyplot as plt
from matplotlib.ticker import (AutoMinorLocator, MultipleLocator)
import numpy as np

start = -10
end = 10

x = np.array([0.0, 10.0])
# x = np.linspace(0, 10, 2)
y = (1/3.15) * x + 0.1;

print(f"({x[0]}, {y[0]})")
print(f"({x[1]}, {y[1]})")

# tmp = x
# x = y
# y = tmp

print(f"({x[0]}, {y[0]})")
print(f"({x[1]}, {y[1]})")

# x = -x

y = -y

print(f"({x[0]}, {y[0]})")
print(f"({x[1]}, {y[1]})")

fig, ax = plt.subplots(figsize=(10, 8))

ax.set_xlim(start, end);
ax.set_ylim(start, end);

ax.xaxis.set_major_locator(MultipleLocator(1));
ax.yaxis.set_major_locator(MultipleLocator(1));

ax.grid(which='major', color='#CCCCCC', linestyle='--')

# fig = plt.figure(figsize = (10, 5))
plt.plot(x, y);
plt.show();
