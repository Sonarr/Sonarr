import _ from 'lodash';
import qs from 'qs';

// See: https://developer.mozilla.org/en-US/docs/Web/API/HTMLHyperlinkElementUtils
const anchor = document.createElement('a');

export default function parseUrl(url: string) {
  anchor.href = url;

  // The `origin`, `password`, and `username` properties are unavailable in
  // Opera Presto. We synthesize `origin` if it's not present. While `password`
  // and `username` are ignored intentionally.
  const properties: Record<string, string | number | boolean | object> = _.pick(
    anchor,
    'hash',
    'host',
    'hostname',
    'href',
    'origin',
    'pathname',
    'port',
    'protocol',
    'search'
  );

  properties.isAbsolute = /^[\w:]*\/\//.test(url);

  if (properties.search) {
    // Remove leading ? from querystring before parsing.
    properties.params = qs.parse((properties.search as string).substring(1));
  } else {
    properties.params = {};
  }

  return properties;
}
