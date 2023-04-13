import { reduce } from 'lodash';
import { SelectedState } from 'Helpers/Hooks/useSelectState';

function getSelectedIds(selectedState: SelectedState): number[] {
  return reduce(
    selectedState,
    (result: number[], value, id) => {
      if (value) {
        result.push(parseInt(id));
      }

      return result;
    },
    []
  );
}

export default getSelectedIds;
