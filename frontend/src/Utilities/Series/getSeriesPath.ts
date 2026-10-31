import Series from 'Series/Series';

function getSeriesPath({ id, titleSlug }: Pick<Series, 'id' | 'titleSlug'>) {
  return `/series/${titleSlug}/${id}`;
}

export default getSeriesPath;
