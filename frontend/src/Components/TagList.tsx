import React from 'react';
import { kinds } from 'Helpers/Props';
import { Kind } from 'Helpers/Props/kinds';
import { Tag } from 'Tags/useTags';
import sortByProp from 'Utilities/Array/sortByProp';
import Label, { LabelProps } from './Label';
import styles from './TagList.css';

interface TagListProps {
  tags: number[];
  tagList: Tag[];
  kind?: Extract<Kind, LabelProps['kind']>;
}

export default function TagList({
  tags,
  tagList,
  kind = kinds.INFO,
}: TagListProps) {
  const sortedTags = tags
    .map((tagId) => tagList.find((tag) => tag.id === tagId))
    .filter((tag) => !!tag)
    .sort(sortByProp('label'));

  return (
    <div className={styles.tags}>
      {sortedTags.map((tag) => {
        return (
          <Label key={tag.id} kind={kind}>
            {tag.label}
          </Label>
        );
      })}
    </div>
  );
}
