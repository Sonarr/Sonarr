import React from 'react';
import { useTagList } from 'Tags/useTags';
import TagList from './TagList';

interface SeriesTagListProps {
  tags: number[];
}

function SeriesTagList({ tags }: SeriesTagListProps) {
  const tagList = useTagList();

  return <TagList tags={tags} tagList={tagList} />;
}

export default SeriesTagList;
