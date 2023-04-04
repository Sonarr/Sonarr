import { reduce } from 'lodash';
import { SelectedState } from 'Helpers/Hooks/useSelectState';

// TODO: This needs to handle string IDs as well
function getSelectedIds(
  selectedState: SelectedState,
  { parseIds = true } = {}
): number[] {
  return reduce(
    selectedState,
    (result: any[], value, id) => {
      if (value) {
        const parsedId = parseIds ? parseInt(id) : id;

        result.push(parsedId);
      }

      return result;
    },
    []
  );
}

export default getSelectedIds;
