// eslint-disable filenames/match-exported

import Fuse from 'fuse.js';
import { SuggestedSeries } from './SeriesSearchInput';

const fuseOptions = {
  shouldSort: true,
  includeMatches: true,
  ignoreLocation: true,
  threshold: 0.3,
  maxPatternLength: 32,
  minMatchCharLength: 1,
  keys: [
    'title',
    'alternateTitles.title',
    'tvdbId',
    'tvMazeId',
    'imdbId',
    'tmdbId',
    'tags.label',
  ],
};

function getSuggestions(series: SuggestedSeries[], value: string) {
  const limit = 10;
  let suggestions = [];

  if (value.length === 1) {
    for (let i = 0; i < series.length; i++) {
      const s = series[i];
      if (s.firstCharacter === value.toLowerCase()) {
        suggestions.push({
          item: series[i],
          indices: [[0, 0]],
          matches: [
            {
              value: s.title,
              key: 'title',
            },
          ],
          refIndex: 0,
        });
        if (suggestions.length > limit) {
          break;
        }
      }
    }
  } else {
    const fuse = new Fuse(series, fuseOptions);
    suggestions = fuse.search(value, { limit });
  }

  return suggestions;
}

onmessage = function (e) {
  if (!e) {
    return;
  }

  const { series, value } = e.data;

  const suggestions = getSuggestions(series, value);

  const results = {
    value,
    suggestions,
  };

  self.postMessage(results);
};
