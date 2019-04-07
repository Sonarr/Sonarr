import { createSelectorCreator, defaultMemoize } from 'reselect';
import _ from 'lodash';

const createDeepEqualSelector = createSelectorCreator(
  defaultMemoize,
  _.isEqual
);

export default createDeepEqualSelector;
