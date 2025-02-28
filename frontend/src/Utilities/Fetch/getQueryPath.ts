import { apiRoot, urlBase } from 'Utilities/Fetch/fetchJson';

const getQueryPath = (path: string) => {
  return urlBase + apiRoot + path;
};

export default getQueryPath;
