function getRemovedItems(prevItems, currentItems, idProp = 'id') {
  if (prevItems === currentItems) {
    return [];
  }

  const currentItemIds = new Set();

  currentItems.forEach((currentItem) => {
    currentItemIds.add(currentItem[idProp]);
  });

  return prevItems.filter((prevItem) => !currentItemIds.has(prevItem[idProp]));
}

export default getRemovedItems;
