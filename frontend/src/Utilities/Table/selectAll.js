import _ from 'lodash';

function selectAll(selectedState, selected) {
  const newSelectedState = _.reduce(Object.keys(selectedState), (result, item) => {
    result[item] = selected;
    return result;
  }, {});

  return {
    allSelected: selected,
    allUnselected: !selected,
    lastToggled: null,
    selectedState: newSelectedState
  };
}

export default selectAll;
