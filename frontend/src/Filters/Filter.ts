import ModelBase from 'App/ModelBase';
import { FilterBuilderTypes } from 'Helpers/Props/filterBuilderTypes';
import { DateFilterValue, FilterType } from 'Helpers/Props/filterTypes';

export interface FilterBuilderPropOption {
  id: string;
  name: string;
}

export interface FilterBuilderProp<T> {
  name: string;
  label: string | (() => string);
  type: FilterBuilderTypes;
  valueType?: string;
  optionsSelector?: (items: T[]) => FilterBuilderPropOption[];
}

export interface PropertyFilter {
  key: string;
  value: string | string[] | number[] | boolean[] | DateFilterValue;
  type: FilterType;
}

export interface Filter {
  key: string;
  label: string | (() => string);
  filters: PropertyFilter[];
}

export interface CustomFilter extends ModelBase {
  type: string;
  label: string;
  filters: PropertyFilter[];
}
