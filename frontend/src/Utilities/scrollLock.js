// Allow iOS devices to disable scrolling of the body/virtual table
// when a modal is open. This will prevent focusing an input in a
// modal causing the modal to close due to scrolling.

let scrollLock = false;

export function isLocked() {
  return scrollLock;
}

export function setScrollLock(locked) {
  scrollLock = locked;
}
