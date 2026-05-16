import matplotlib.pyplot as plt
from matplotlib.ticker import (AutoMinorLocator, MultipleLocator)
import numpy as np

x = np.linspace(0, 10, 2)
# y = (1/3.15) * x + 0.1;
y = (1/2) * x + 0;

print(x)
print(y)


fig, ax = plt.subplots(figsize=(10, 8))

ax.set_xlim(0, 10);
ax.set_ylim(0, 10);

ax.xaxis.set_major_locator(MultipleLocator(1));
ax.yaxis.set_major_locator(MultipleLocator(1));

ax.grid(which='major', color='#CCCCCC', linestyle='--')

# fig = plt.figure(figsize = (10, 5))
plt.plot(x, y);
plt.show();
