import * as filterTypes from './filterTypes';

export const ARRAY = 'array';
export const DATE = 'date';
export const EXACT = 'exact';
export const NUMBER = 'number';
export const STRING = 'string';

export const all = [
  ARRAY,
  DATE,
  EXACT,
  NUMBER,
  STRING
];

export const possibleFilterTypes = {
  [ARRAY]: [
    { key: filterTypes.CONTAINS, value: 'contains' },
    { key: filterTypes.NOT_CONTAINS, value: 'does not contain' }
  ],

  [DATE]: [
    { key: filterTypes.LESS_THAN, value: 'is before' },
    { key: filterTypes.GREATER_THAN, value: 'is after' },
    { key: filterTypes.IN_LAST, value: 'in the last' },
    { key: filterTypes.IN_NEXT, value: 'in the next' }
  ],

  [EXACT]: [
    { key: filterTypes.EQUAL, value: 'is' },
    { key: filterTypes.NOT_EQUAL, value: 'is not' }
  ],

  [NUMBER]: [
    { key: filterTypes.EQUAL, value: 'equal' },
    { key: filterTypes.GREATER_THAN, value: 'greater than' },
    { key: filterTypes.GREATER_THAN_OR_EQUAL, value: 'greater than or equal' },
    { key: filterTypes.LESS_THAN, value: 'less than' },
    { key: filterTypes.LESS_THAN_OR_EQUAL, value: 'less than or equal' },
    { key: filterTypes.NOT_EQUAL, value: 'not equal' }
  ],

  [STRING]: [
    { key: filterTypes.CONTAINS, value: 'contains' },
    { key: filterTypes.NOT_CONTAINS, value: 'does not contain' },
    { key: filterTypes.EQUAL, value: 'equal' },
    { key: filterTypes.NOT_EQUAL, value: 'not equal' }
  ]
};
