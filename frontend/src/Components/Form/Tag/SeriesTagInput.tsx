import React, { useCallback, useMemo } from 'react';
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

interface SeriesTagInputProps {
  name: string;
  value: number | number[];
  onChange: (change: InputChanged<number | number[]>) => void;
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
  const isArray = Array.isArray(value);

  const arrayValue = useMemo(() => {
    if (isArray) {
      return value;
    }

    return value === 0 ? [] : [value];
  }, [isArray, value]);

  const { tags, tagList, allTags } = useSelector(
    createSeriesTagsSelector(arrayValue)
  );

  const handleTagCreated = useCallback(
    (tag: SeriesTag) => {
      if (isArray) {
        onChange({ name, value: [...value, tag.id] });
      } else {
        onChange({
          name,
          value: tag.id,
        });
      }
    },
    [name, value, isArray, onChange]
  );

  const handleTagAdd = useCallback(
    (newTag: SeriesTag) => {
      if (newTag.id) {
        if (isArray) {
          onChange({ name, value: [...value, newTag.id] });
        } else {
          onChange({ name, value: newTag.id });
        }

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
    [name, value, isArray, allTags, handleTagCreated, onChange, dispatch]
  );

  const handleTagDelete = useCallback(
    ({ index }: { index: number }) => {
      if (isArray) {
        const newValue = value.slice();
        newValue.splice(index, 1);

        onChange({ name, value: newValue });
      } else {
        onChange({ name, value: 0 });
      }
    },
    [name, value, isArray, onChange]
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
