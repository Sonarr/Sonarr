import _ from 'lodash';
import { createSelectorCreator, defaultMemoize } from 'reselect';

const createDeepEqualSelector = createSelectorCreator(
  defaultMemoize,
  _.isEqual
);

export default createDeepEqualSelector;
