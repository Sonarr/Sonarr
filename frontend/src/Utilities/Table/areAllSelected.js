export default function areAllSelected(selectedState) {
  let allSelected = true;
  let allUnselected = true;

  Object.keys(selectedState).forEach((key) => {
    if (selectedState[key]) {
      allUnselected = false;
    } else {
      allSelected = false;
    }
  });

  return {
    allSelected,
    allUnselected
  };
}
