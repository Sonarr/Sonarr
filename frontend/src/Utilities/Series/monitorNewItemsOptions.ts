import { EnhancedSelectInputValue } from 'Components/Form/Select/EnhancedSelectInput';
import translate from 'Utilities/String/translate';

const monitorNewItemsOptions: EnhancedSelectInputValue<string>[] = [
  {
    key: 'all',
    get value() {
      return translate('MonitorAllSeasons');
    },
  },
  {
    key: 'none',
    get value() {
      return translate('MonitorNoNewSeasons');
    },
  },
];

export default monitorNewItemsOptions;
