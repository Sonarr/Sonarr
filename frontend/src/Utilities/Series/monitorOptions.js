import translate from 'Utilities/String/translate';

const monitorOptions = [
  {
    key: 'all',
    get value() {
      return translate('MonitorAllEpisodes');
    }
  },
  {
    key: 'future',
    get value() {
      return translate('MonitorFutureEpisodes');
    }
  },
  {
    key: 'missing',
    get value() {
      return translate('MonitorMissingEpisodes');
    }
  },
  {
    key: 'existing',
    get value() {
      return translate('MonitorExistingEpisodes');
    }
  },
  {
    key: 'pilot',
    get value() {
      return translate('MonitorPilotEpisode');
    }
  },
  {
    key: 'firstSeason',
    get value() {
      return translate('MonitorFirstSeason');
    }
  },
  {
    key: 'latestSeason',
    get value() {
      return translate('MonitorLatestSeason');
    }
  },
  {
    key: 'monitorSpecials',
    get value() {
      return translate('MonitorSpecials');
    }
  },
  {
    key: 'unmonitorSpecials',
    get value() {
      return translate('UnmonitorSpecials');
    }
  },
  {
    key: 'none',
    get value() {
      return translate('MonitorNone');
    }
  }
];

export default monitorOptions;
