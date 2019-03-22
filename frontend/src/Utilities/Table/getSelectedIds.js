import _ from 'lodash';

function getSelectedIds(selectedState, { parseIds = true } = {}) {
  return _.reduce(selectedState, (result, value, id) => {
    if (value) {
      const parsedId = parseIds ? parseInt(id) : id;

      result.push(parsedId);
    }

    return result;
  }, []);
}

export default getSelectedIds;
