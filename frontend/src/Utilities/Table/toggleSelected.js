import areAllSelected from './areAllSelected';
import getToggledRange from './getToggledRange';

function toggleSelected(state, items, id, selected, shiftKey) {
  const lastToggled = state.lastToggled;
  const selectedState = {
    ...state.selectedState,
    [id]: selected
  };

  if (selected == null) {
    delete selectedState[id];
  }

  if (shiftKey && lastToggled) {
    const { lower, upper } = getToggledRange(items, id, lastToggled);

    for (let i = lower; i < upper; i++) {
      selectedState[items[i].id] = selected;
    }
  }

  return {
    ...areAllSelected(selectedState),
    lastToggled: id,
    selectedState
  };
}

export default toggleSelected;
