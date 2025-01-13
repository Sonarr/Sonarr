import { reduce } from 'lodash';
import { SelectedState } from 'Helpers/Hooks/useSelectState';

function getSelectedIds<T extends number | string = number>(
  selectedState: SelectedState,
  idParser: (id: string) => T = (id) => parseInt(id) as T
): T[] {
  return reduce(
    selectedState,
    (result: T[], value, id) => {
      if (value) {
        result.push(idParser(id));
      }

      return result;
    },
    []
  );
}

export default getSelectedIds;
