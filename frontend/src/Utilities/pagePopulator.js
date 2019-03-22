let currentPopulator = null;
let currentReasons = [];

export function registerPagePopulator(populator, reasons = []) {
  currentPopulator = populator;
  currentReasons = reasons;
}

export function unregisterPagePopulator(populator) {
  if (currentPopulator === populator) {
    currentPopulator = null;
    currentReasons = [];
  }
}

export function repopulatePage(reason) {
  if (!currentPopulator) {
    return;
  }

  if (!reason) {
    currentPopulator();
  }

  if (reason && currentReasons.includes(reason)) {
    currentPopulator();
  }
}
