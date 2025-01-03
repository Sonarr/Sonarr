import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import { addTag } from 'Store/Actions/tagActions';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import { InputChanged } from 'typings/inputs';
import sortByProp from 'Utilities/Array/sortByProp';
import TagInput, { TagBase } from './TagInput';

interface SeriesTag extends TagBase {
  id: number;
  name: string;
}

export interface SeriesTagInputProps {
  name: string;
  value: number[];
  onChange: (change: InputChanged<number[]>) => void;
}

const VALID_TAG_REGEX = new RegExp('[^-_a-z0-9]', 'i');

function isValidTag(tagName: string) {
  try {
    return !VALID_TAG_REGEX.test(tagName);
  } catch {
    return false;
  }
}

function createSeriesTagsSelector(tags: number[]) {
  return createSelector(createTagsSelector(), (tagList) => {
    const sortedTags = tagList.sort(sortByProp('label'));
    const filteredTagList = sortedTags.filter((tag) => !tags.includes(tag.id));

    return {
      tags: tags.reduce((acc: SeriesTag[], tag) => {
        const matchingTag = tagList.find((t) => t.id === tag);

        if (matchingTag) {
          acc.push({
            id: tag,
            name: matchingTag.label,
          });
        }

        return acc;
      }, []),

      tagList: filteredTagList.map(({ id, label: name }) => {
        return {
          id,
          name,
        };
      }),

      allTags: sortedTags,
    };
  });
}

export default function SeriesTagInput({
  name,
  value,
  onChange,
}: SeriesTagInputProps) {
  const dispatch = useDispatch();

  const { tags, tagList, allTags } = useSelector(
    createSeriesTagsSelector(value)
  );

  const handleTagCreated = useCallback(
    (tag: SeriesTag) => {
      onChange({ name, value: [...value, tag.id] });
    },
    [name, value, onChange]
  );

  const handleTagAdd = useCallback(
    (newTag: SeriesTag) => {
      if (newTag.id) {
        onChange({ name, value: [...value, newTag.id] });

        return;
      }

      const existingTag = allTags.some((t) => t.label === newTag.name);

      if (isValidTag(newTag.name) && !existingTag) {
        dispatch(
          addTag({
            tag: { label: newTag.name },
            onTagCreated: handleTagCreated,
          })
        );
      }
    },
    [name, value, allTags, handleTagCreated, onChange, dispatch]
  );

  const handleTagDelete = useCallback(
    ({ index }: { index: number }) => {
      const newValue = value.slice();
      newValue.splice(index, 1);

      onChange({ name, value: newValue });
    },
    [name, value, onChange]
  );

  return (
    <TagInput
      name={name}
      tags={tags}
      tagList={tagList}
      onTagAdd={handleTagAdd}
      onTagDelete={handleTagDelete}
    />
  );
}
