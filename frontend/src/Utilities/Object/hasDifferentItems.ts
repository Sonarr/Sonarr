import ModelBase from 'App/ModelBase';

function hasDifferentItems<T extends ModelBase>(
  prevItems: T[],
  currentItems: T[],
  idProp: keyof T = 'id'
) {
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
