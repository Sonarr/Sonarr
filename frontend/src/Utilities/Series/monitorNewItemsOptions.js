import translate from 'Utilities/String/translate';

const monitorNewItemsOptions = [
  {
    key: 'all',
    get value() {
      return translate('MonitorAllSeasons');
    }
  },
  {
    key: 'none',
    get value() {
      return translate('MonitorNoNewSeasons');
    }
  }
];

export default monitorNewItemsOptions;
