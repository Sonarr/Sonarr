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

  const firstSeason = _.minBy(_.reject(seasons, { seasonNumber: 0 }), 'seasonNumber');
  const lastSeason = _.maxBy(seasons, 'seasonNumber');

  const firstSeasonNumber = firstSeason ? firstSeason.seasonNumber : 1;
  const lastSeasonNumber = lastSeason ? lastSeason.seasonNumber : 1;

  monitorSeasons(seasons, firstSeasonNumber);

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
      monitorSeasons(seasons, lastSeasonNumber);
      break;
    case 'first':
      monitorSeasons(seasons, lastSeasonNumber + 1);
      _.find(seasons, { seasonNumber: firstSeasonNumber }).monitored = true;
      break;
    case 'missing':
      monitoringOptions.ignoreEpisodesWithFiles = true;
      break;
    case 'existing':
      monitoringOptions.ignoreEpisodesWithoutFiles = true;
      break;
    case 'none':
      monitorSeasons(seasons, lastSeasonNumber + 1);
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
