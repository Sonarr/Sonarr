import translate from 'Utilities/String/translate';
import { FilterType } from './filterTypes';

export const ARRAY = 'array';
export const CONTAINS = 'contains';
export const DATE = 'date';
export const EQUAL = 'equal';
export const EXACT = 'exact';
export const NUMBER = 'number';
export const STRING = 'string';

export const all = [ARRAY, CONTAINS, DATE, EQUAL, EXACT, NUMBER, STRING];

interface FilterBuilderTypeOption {
  key: FilterType;
  value: () => string;
}

export type FilterBuilderTypes =
  | 'array'
  | 'contains'
  | 'date'
  | 'equal'
  | 'exact'
  | 'number'
  | 'string';

export const possibleFilterTypes: Record<
  FilterBuilderTypes,
  FilterBuilderTypeOption[]
> = {
  array: [
    {
      key: 'contains',
      value: () => translate('FilterContains'),
    },
    {
      key: 'notContains',
      value: () => translate('FilterDoesNotContain'),
    },
  ],

  contains: [
    {
      key: 'contains',
      value: () => translate('FilterContains'),
    },
  ],

  date: [
    {
      key: 'lessThan',
      value: () => translate('FilterIsBefore'),
    },
    {
      key: 'greaterThan',
      value: () => translate('FilterIsAfter'),
    },
    {
      key: 'inLast',
      value: () => translate('FilterInLast'),
    },
    {
      key: 'notInLast',
      value: () => translate('FilterNotInLast'),
    },
    {
      key: 'inNext',
      value: () => translate('FilterInNext'),
    },
    {
      key: 'notInNext',
      value: () => translate('FilterNotInNext'),
    },
  ],

  equal: [
    {
      key: 'equal',
      value: () => translate('FilterIs'),
    },
  ],

  exact: [
    {
      key: 'equal',
      value: () => translate('FilterIs'),
    },
    {
      key: 'notEqual',
      value: () => translate('FilterIsNot'),
    },
  ],

  number: [
    {
      key: 'equal',
      value: () => translate('FilterEqual'),
    },
    {
      key: 'greaterThan',
      value: () => translate('FilterGreaterThan'),
    },
    {
      key: 'greaterThanOrEqual',
      value: () => translate('FilterGreaterThanOrEqual'),
    },
    {
      key: 'lessThan',
      value: () => translate('FilterLessThan'),
    },
    {
      key: 'lessThanOrEqual',
      value: () => translate('FilterLessThanOrEqual'),
    },
    {
      key: 'notEqual',
      value: () => translate('FilterNotEqual'),
    },
  ],

  string: [
    {
      key: 'contains',
      value: () => translate('FilterContains'),
    },
    {
      key: 'notContains',
      value: () => translate('FilterDoesNotContain'),
    },
    {
      key: 'equal',
      value: () => translate('FilterEqual'),
    },
    {
      key: 'notEqual',
      value: () => translate('FilterNotEqual'),
    },
    {
      key: 'startsWith',
      value: () => translate('FilterStartsWith'),
    },
    {
      key: 'notStartsWith',
      value: () => translate('FilterDoesNotStartWith'),
    },
    {
      key: 'endsWith',
      value: () => translate('FilterEndsWith'),
    },
    {
      key: 'notEndsWith',
      value: () => translate('FilterDoesNotEndWith'),
    },
  ],
};
