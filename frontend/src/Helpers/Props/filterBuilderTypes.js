import * as filterTypes from './filterTypes';

export const EXACT = 'exact';
export const NUMBER = 'number';
export const STRING = 'string';

export const all = [
  EXACT,
  NUMBER,
  STRING
];

export const possibleFilterTypes = {
  [EXACT]: [
    { key: filterTypes.EQUAL, value: 'Is' },
    { key: filterTypes.NOT_EQUAL, value: 'Is Not' }
  ],

  [NUMBER]: [
    { key: filterTypes.EQUAL, value: 'Equal' },
    { key: filterTypes.GREATER_THAN, value: 'Greater Than' },
    { key: filterTypes.GREATER_THAN_OR_EQUAL, value: 'Greater Than or Equal' },
    { key: filterTypes.LESS_THAN, value: 'Less Than' },
    { key: filterTypes.LESS_THAN_OR_EQUAL, value: 'Less Than or Equal' },
    { key: filterTypes.NOT_EQUAL, value: 'Not Equal' }
  ],

  [STRING]: [
    { key: filterTypes.CONTAINS, value: 'Contains' },
    { key: filterTypes.EQUAL, value: 'Equal' },
    { key: filterTypes.NOT_EQUAL, value: 'Not Equal' }
  ]
};
