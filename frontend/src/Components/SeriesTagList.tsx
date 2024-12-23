import React from 'react';
import { useSelector } from 'react-redux';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import TagList from './TagList';

interface SeriesTagListProps {
  tags: number[];
}

function SeriesTagList({ tags }: SeriesTagListProps) {
  const tagList = useSelector(createTagsSelector());

  return <TagList tags={tags} tagList={tagList} />;
}

export default SeriesTagList;
