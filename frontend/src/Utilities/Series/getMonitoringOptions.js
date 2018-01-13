import _ from 'lodash';

function monitorSeasons(seasons, startingSeason) {
  seasons.forEach((season) => {
    if (season.seasonNumber >= startingSeason) {
      season.monitored = true;
    } else {
      season.monitored = false;
    }
  });
}

function getMonitoringOptions(seasons, monitor) {
  if (!seasons.length) {
    return {
      seasons: [],
      options: {
        ignoreEpisodesWithFiles: false,
        ignoreEpisodesWithoutFiles: false
      }
    };
  }

  const firstSeason = _.minBy(_.reject(seasons, { seasonNumber: 0 }), 'seasonNumber').seasonNumber;
  const lastSeason = _.maxBy(seasons, 'seasonNumber').seasonNumber;

  monitorSeasons(seasons, firstSeason);

  const monitoringOptions = {
    ignoreEpisodesWithFiles: false,
    ignoreEpisodesWithoutFiles: false
  };

  switch (monitor) {
    case 'future':
      monitoringOptions.ignoreEpisodesWithFiles = true;
      monitoringOptions.ignoreEpisodesWithoutFiles = true;
      break;
    case 'latest':
      monitorSeasons(seasons, lastSeason);
      break;
    case 'first':
      monitorSeasons(seasons, lastSeason + 1);
      _.find(seasons, { seasonNumber: firstSeason }).monitored = true;
      break;
    case 'missing':
      monitoringOptions.ignoreEpisodesWithFiles = true;
      break;
    case 'existing':
      monitoringOptions.ignoreEpisodesWithoutFiles = true;
      break;
    case 'none':
      monitorSeasons(seasons, lastSeason + 1);
      break;
    default:
      break;
  }

  return {
    seasons: _.map(seasons, (season) => {
      return _.pick(season, [
        'seasonNumber',
        'monitored'
      ]);
    }),
    options: monitoringOptions
  };
}

export default getMonitoringOptions;
