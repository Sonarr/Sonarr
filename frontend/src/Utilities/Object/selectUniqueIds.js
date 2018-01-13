import _ from 'lodash';

function selectUniqueIds(items, idProp) {
  const ids = _.reduce(items, (result, item) => {
    if (item[idProp]) {
      result.push(item[idProp]);
    }

    return result;
  }, []);

  return _.uniq(ids);
}

export default selectUniqueIds;
