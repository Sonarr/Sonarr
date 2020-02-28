function hasDifferentItems(prevItems, currentItems, idProp = 'id') {
  if (prevItems === currentItems) {
    return false;
  }

  if (prevItems.length !== currentItems.length) {
    return true;
  }

  const currentItemIds = new Set();

  currentItems.forEach((currentItem) => {
    currentItemIds.add(currentItem[idProp]);
  });

  return prevItems.some((prevItem) => !currentItemIds.has(prevItem[idProp]));
}

export default hasDifferentItems;
