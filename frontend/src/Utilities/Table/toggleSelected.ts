import getToggledRange from './getToggledRange';

interface SelectState {
  allSelected: boolean;
  allUnselected: boolean;
  lastToggled: number | string | null;
  selectedState: SelectedState;
}

type SelectedState = Record<number | string, boolean>;

interface SelectStateModel {
  id: number | string;
}

function areAllSelected(selectedState: SelectedState) {
  let allSelected = true;
  let allUnselected = true;

  Object.values(selectedState).forEach((value) => {
    if (value) {
      allUnselected = false;
    } else {
      allSelected = false;
    }
  });

  return {
    allSelected,
    allUnselected,
  };
}

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
