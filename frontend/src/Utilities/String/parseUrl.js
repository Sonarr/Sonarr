import _ from 'lodash';
import qs from 'qs';

// See: https://developer.mozilla.org/en-US/docs/Web/API/HTMLHyperlinkElementUtils
const anchor = document.createElement('a');

export default function parseUrl(url) {
  anchor.href = url;

  // The `origin`, `password`, and `username` properties are unavailable in
  // Opera Presto. We synthesize `origin` if it's not present. While `password`
  // and `username` are ignored intentionally.
  const properties = _.pick(
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

  properties.isAbsolute = (/^[\w:]*\/\//).test(url);

  if (properties.search) {
    // Remove leading ? from querystring before parsing.
    properties.params = qs.parse(properties.search.substring(1));
  }

  return properties;
}
