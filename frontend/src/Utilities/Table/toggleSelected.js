import areAllSelected from './areAllSelected';
import getToggledRange from './getToggledRange';

function toggleSelected(selectedState, items, id, selected, shiftKey) {
  const lastToggled = selectedState.lastToggled;
  const nextSelectedState = {
    ...selectedState.selectedState,
    [id]: selected
  };

  if (selected == null) {
    delete nextSelectedState[id];
  }

  if (shiftKey && lastToggled) {
    const { lower, upper } = getToggledRange(items, id, lastToggled);

    for (let i = lower; i < upper; i++) {
      nextSelectedState[items[i].id] = selected;
    }
  }

  return {
    ...areAllSelected(nextSelectedState),
    lastToggled: id,
    selectedState: nextSelectedState
  };
}

export default toggleSelected;
