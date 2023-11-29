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
    key: 'recent',
    get value() {
      return translate('MonitorRecentEpisodes');
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
    key: 'lastSeason',
    get value() {
      return translate('MonitorLastSeason');
    }
  },
  {
    key: 'monitorSpecials',
    get value() {
      return translate('MonitorSpecialEpisodes');
    }
  },
  {
    key: 'unmonitorSpecials',
    get value() {
      return translate('UnmonitorSpecialEpisodes');
    }
  },
  {
    key: 'none',
    get value() {
      return translate('MonitorNoEpisodes');
    }
  }
];

export default monitorOptions;
