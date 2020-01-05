import Fuse from 'fuse.js';

const fuseOptions = {
  shouldSort: true,
  includeMatches: true,
  threshold: 0.3,
  location: 0,
  distance: 100,
  maxPatternLength: 32,
  minMatchCharLength: 1,
  keys: [
    'title',
    'alternateTitles.title',
    'tags.label'
  ]
};

function getSuggestions(series, value) {
  const limit = 10;
  let suggestions = [];

  if (value.length === 1) {
    for (let i = 0; i < series.length; i++) {
      const s = series[i];
      if (s.firstCharacter === value.toLowerCase()) {
        suggestions.push({
          item: series[i],
          indices: [
            [0, 0]
          ],
          matches: [
            {
              value: s.title,
              key: 'title'
            }
          ],
          arrayIndex: 0
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

self.addEventListener('message', (e) => {
  if (!e) {
    return;
  }

  const {
    series,
    value
  } = e.data;

  self.postMessage(getSuggestions(series, value));
});
