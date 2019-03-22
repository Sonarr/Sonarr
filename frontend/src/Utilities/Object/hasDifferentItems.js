import _ from 'lodash';

function hasDifferentItems(prevItems, currentItems, idProp = 'id') {
  const diff1 = _.differenceBy(prevItems, currentItems, (item) => item[idProp]);
  const diff2 = _.differenceBy(currentItems, prevItems, (item) => item[idProp]);

  return diff1.length > 0 || diff2.length > 0;
}

export default hasDifferentItems;
