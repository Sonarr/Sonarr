import { EnhancedSelectInputValue } from 'Components/Form/Select/EnhancedSelectInput';
import translate from 'Utilities/String/translate';
import { TagExisting } from './useImportLists';

const tagExistingOptions: EnhancedSelectInputValue<TagExisting>[] = [
  {
    key: 'none',
    get value() {
      return translate('TagExistingNone');
    },
    get hint() {
      return translate('TagExistingNoneHint');
    },
  },
  {
    key: 'add',
    get value() {
      return translate('TagExistingAdd');
    },
    get hint() {
      return translate('TagExistingAddHint');
    },
  },
  {
    key: 'sync',
    get value() {
      return translate('TagExistingSync');
    },
    get hint() {
      return translate('TagExistingSyncHint');
    },
  },
];

export default tagExistingOptions;
