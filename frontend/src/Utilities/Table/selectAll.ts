import { SelectedState } from 'Helpers/Hooks/useSelectState';

function selectAll(selectedState: SelectedState, selected: boolean) {
  const newSelectedState = Object.keys(selectedState).reduce<
    Record<number | string, boolean>
  >((acc, item) => {
    acc[item] = selected;
    return acc;
  }, {});

  return {
    allSelected: selected,
    allUnselected: !selected,
    lastToggled: null,
    selectedState: newSelectedState,
  };
}

export default selectAll;
