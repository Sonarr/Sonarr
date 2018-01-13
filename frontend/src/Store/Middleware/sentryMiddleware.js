import _ from 'lodash';
import ravenMiddleware from 'redux-raven-middleware';
import parseUrl from 'Utilities/String/parseUrl';

function cleanseUrl(url) {
  const properties = parseUrl(url);

  return `${properties.pathname}${properties.search}`;
}

function cleanseData(data) {
  const result = _.cloneDeep(data);

  result.culprit = cleanseUrl(result.culprit);
  result.request.url = cleanseUrl(result.request.url);

  return result;
}

export default function sentryMiddleware() {
  const {
    analytics,
    branch,
    version,
    release,
    isProduction
  } = window.Sonarr;

  if (!analytics) {
    return;
  }

  const dsn = isProduction ? 'https://b80ca60625b443c38b242e0d21681eb7@sentry.sonarr.tv/13' :
    'https://8dbaacdfe2ff4caf97dc7945aecf9ace@sentry.sonarr.tv/12';

  return ravenMiddleware(dsn, {
    environment: isProduction ? 'production' : 'development',
    release,
    tags: {
      branch,
      version
    },
    dataCallback: cleanseData
  });
}
