import _ from 'lodash';

function split(input, separator = ',') {
  if (!input) {
    return [];
  }

  return _.reduce(input.split(separator), (result, s) => {
    if (s) {
      result.push(s);
    }

    return result;
  }, []);
}

export default split;
