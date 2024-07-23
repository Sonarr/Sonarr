import ModelBase from 'App/ModelBase';
import { SelectState } from 'Helpers/Hooks/useSelectState';
import areAllSelected from './areAllSelected';

export default function removeOldSelectedState<T extends ModelBase>(
  state: SelectState,
  prevItems: T[]
) {
  const selectedState = {
    ...state.selectedState,
  };

  prevItems.forEach((item) => {
    delete selectedState[item.id];
  });

  return {
    ...areAllSelected(selectedState),
    selectedState,
  };
}
