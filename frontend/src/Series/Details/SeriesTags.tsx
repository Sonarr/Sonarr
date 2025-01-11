import React from 'react';
import Label from 'Components/Label';
import { kinds, sizes } from 'Helpers/Props';
import useSeries from 'Series/useSeries';
import useTags from 'Tags/useTags';
import sortByProp from 'Utilities/Array/sortByProp';

interface SeriesTagsProps {
  seriesId: number;
}

function SeriesTags({ seriesId }: SeriesTagsProps) {
  const series = useSeries(seriesId)!;
  const tagList = useTags();

  const tags = series.tags
    .map((tagId) => tagList.find((tag) => tag.id === tagId))
    .filter((tag) => !!tag)
    .sort(sortByProp('label'))
    .map((tag) => tag.label);

  return (
    <div>
      {tags.map((tag) => {
        return (
          <Label key={tag} kind={kinds.INFO} size={sizes.LARGE}>
            {tag}
          </Label>
        );
      })}
    </div>
  );
}

export default SeriesTags;
