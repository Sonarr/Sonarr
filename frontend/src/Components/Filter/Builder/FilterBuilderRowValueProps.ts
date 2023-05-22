import { FilterBuilderProp } from 'App/State/AppState';

interface FilterBuilderRowOnChangeProps {
  name: string;
  value: unknown[];
}

interface FilterBuilderRowValueProps {
  filterType?: string;
  filterValue: string | number | object | string[] | number[] | object[];
  selectedFilterBuilderProp: FilterBuilderProp<unknown>;
  sectionItem: unknown[];
  onChange: (payload: FilterBuilderRowOnChangeProps) => void;
}

export default FilterBuilderRowValueProps;
