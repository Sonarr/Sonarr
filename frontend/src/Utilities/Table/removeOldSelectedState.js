import areAllSelected from './areAllSelected';

export default function removeOldSelectedState(state, prevItems) {
  const selectedState = {
    ...state.selectedState
  };

  prevItems.forEach((item) => {
    delete selectedState[item.id];
  });

  return {
    ...areAllSelected(selectedState),
    selectedState
  };
}
