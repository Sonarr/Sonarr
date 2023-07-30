import { isEqual } from 'lodash';
import { createSelectorCreator, defaultMemoize } from 'reselect';

const createDeepEqualSelector = createSelectorCreator(defaultMemoize, isEqual);

export default createDeepEqualSelector;
