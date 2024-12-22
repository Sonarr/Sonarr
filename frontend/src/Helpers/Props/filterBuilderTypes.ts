import translate from 'Utilities/String/translate';
import * as filterTypes from './filterTypes';

export const ARRAY = 'array';
export const CONTAINS = 'contains';
export const DATE = 'date';
export const EQUAL = 'equal';
export const EXACT = 'exact';
export const NUMBER = 'number';
export const STRING = 'string';

export const all = [ARRAY, CONTAINS, DATE, EQUAL, EXACT, NUMBER, STRING];

export type FilterBuilderTypes =
  | 'array'
  | 'contains'
  | 'date'
  | 'equal'
  | 'exact'
  | 'number'
  | 'string';

export const possibleFilterTypes = {
  array: [
    {
      key: filterTypes.CONTAINS,
      value: () => translate('FilterContains'),
    },
    {
      key: filterTypes.NOT_CONTAINS,
      value: () => translate('FilterDoesNotContain'),
    },
  ],

  contains: [
    {
      key: filterTypes.CONTAINS,
      value: () => translate('FilterContains'),
    },
  ],

  date: [
    {
      key: filterTypes.LESS_THAN,
      value: () => translate('FilterIsBefore'),
    },
    {
      key: filterTypes.GREATER_THAN,
      value: () => translate('FilterIsAfter'),
    },
    {
      key: filterTypes.IN_LAST,
      value: () => translate('FilterInLast'),
    },
    {
      key: filterTypes.NOT_IN_LAST,
      value: () => translate('FilterNotInLast'),
    },
    {
      key: filterTypes.IN_NEXT,
      value: () => translate('FilterInNext'),
    },
    {
      key: filterTypes.NOT_IN_NEXT,
      value: () => translate('FilterNotInNext'),
    },
  ],

  equal: [
    {
      key: filterTypes.EQUAL,
      value: () => translate('FilterIs'),
    },
  ],

  exact: [
    {
      key: filterTypes.EQUAL,
      value: () => translate('FilterIs'),
    },
    {
      key: filterTypes.NOT_EQUAL,
      value: () => translate('FilterIsNot'),
    },
  ],

  number: [
    {
      key: filterTypes.EQUAL,
      value: () => translate('FilterEqual'),
    },
    {
      key: filterTypes.GREATER_THAN,
      value: () => translate('FilterGreaterThan'),
    },
    {
      key: filterTypes.GREATER_THAN_OR_EQUAL,
      value: () => translate('FilterGreaterThanOrEqual'),
    },
    {
      key: filterTypes.LESS_THAN,
      value: () => translate('FilterLessThan'),
    },
    {
      key: filterTypes.LESS_THAN_OR_EQUAL,
      value: () => translate('FilterLessThanOrEqual'),
    },
    {
      key: filterTypes.NOT_EQUAL,
      value: () => translate('FilterNotEqual'),
    },
  ],

  string: [
    {
      key: filterTypes.CONTAINS,
      value: () => translate('FilterContains'),
    },
    {
      key: filterTypes.NOT_CONTAINS,
      value: () => translate('FilterDoesNotContain'),
    },
    {
      key: filterTypes.EQUAL,
      value: () => translate('FilterEqual'),
    },
    {
      key: filterTypes.NOT_EQUAL,
      value: () => translate('FilterNotEqual'),
    },
    {
      key: filterTypes.STARTS_WITH,
      value: () => translate('FilterStartsWith'),
    },
    {
      key: filterTypes.NOT_STARTS_WITH,
      value: () => translate('FilterDoesNotStartWith'),
    },
    {
      key: filterTypes.ENDS_WITH,
      value: () => translate('FilterEndsWith'),
    },
    {
      key: filterTypes.NOT_ENDS_WITH,
      value: () => translate('FilterDoesNotEndWith'),
    },
  ],
};
