import _ from 'lodash';

function getToggledRange(items, id, lastToggled) {
  const lastToggledIndex = _.findIndex(items, { id: lastToggled });
  const changedIndex = _.findIndex(items, { id });
  let lower = 0;
  let upper = 0;

  if (lastToggledIndex > changedIndex) {
    lower = changedIndex;
    upper = lastToggledIndex + 1;
  } else {
    lower = lastToggledIndex;
    upper = changedIndex;
  }

  return {
    lower,
    upper
  };
}

export default getToggledRange;
