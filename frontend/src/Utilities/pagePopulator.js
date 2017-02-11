let currentPopulator = null;

export function registerPagePopulator(populator) {
  currentPopulator = populator;
}

export function unregisterPagePopulator(populator) {
  if (currentPopulator === populator) {
    currentPopulator = null;
  }
}

export function repopulatePage() {
  if (currentPopulator) {
    currentPopulator();
  }
}
