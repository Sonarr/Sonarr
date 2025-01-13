import { SelectState, SelectStateModel } from 'Helpers/Hooks/useSelectState';
import areAllSelected from './areAllSelected';
import getToggledRange from './getToggledRange';

function toggleSelected<T extends SelectStateModel>(
  selectState: SelectState,
  items: T[],
  id: number | string,
  selected: boolean | null,
  shiftKey: boolean
) {
  const lastToggled = selectState.lastToggled;
  const nextSelectedState = {
    ...selectState.selectedState,
  };

  if (selected == null) {
    delete nextSelectedState[id];
  } else {
    nextSelectedState[id] = selected;

    if (shiftKey && lastToggled) {
      const { lower, upper } = getToggledRange(items, id, lastToggled);

      for (let i = lower; i < upper; i++) {
        nextSelectedState[items[i].id] = selected;
      }
    }
  }

  return {
    ...areAllSelected(nextSelectedState),
    lastToggled: id,
    selectedState: nextSelectedState,
  };
}

export default toggleSelected;
