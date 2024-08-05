import { SelectedState } from 'Helpers/Hooks/useSelectState';

export default function areAllSelected(selectedState: SelectedState) {
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
