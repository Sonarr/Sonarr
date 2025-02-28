import { PropertyFilter } from 'App/State/AppState';

export interface QueryParams {
  [key: string]: string | number | boolean | PropertyFilter[] | undefined;
}

const getQueryString = (queryParams?: QueryParams) => {
  if (!queryParams) {
    return '';
  }

  const filteredParams = Object.keys(queryParams).reduce<
    Record<string, string>
  >((acc, key) => {
    const value = queryParams[key];

    if (value == null) {
      return acc;
    }

    if (Array.isArray(value)) {
      value.forEach((filter) => {
        acc[filter.key] = String(filter.value);
      });
    } else {
      acc[key] = String(value);
    }

    return acc;
  }, {});

  const paramsString = new URLSearchParams(filteredParams).toString();

  return `?${paramsString}`;
};

export default getQueryString;
