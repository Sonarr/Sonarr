import React, { useCallback, useMemo } from 'react';
import { InputChanged } from 'typings/inputs';
import TagInput, { TagBase, TagInputProps } from './TagInput';

interface SelectTag extends TagBase {
  id: number;
  name: string;
}

interface TagSelectValue {
  value: string;
  key: number;
  order: number;
}

export interface TagSelectInputProps extends TagInputProps<SelectTag> {
  name: string;
  value: number[];
  values: TagSelectValue[];
  onChange: (change: InputChanged<number | number[]>) => unknown;
}

function TagSelectInput({
  name,
  value,
  values,
  onChange,
  ...otherProps
}: TagSelectInputProps) {
  const { tags, tagList, allTags } = useMemo(() => {
    const sortedTags = values.sort((a, b) => a.key - b.key);

    return {
      tags: value.reduce((acc: SelectTag[], tag) => {
        const matchingTag = values.find((t) => t.key === tag);

        if (matchingTag) {
          acc.push({
            id: tag,
            name: matchingTag.value,
          });
        }

        return acc;
      }, []),

      tagList: sortedTags.map((sorted) => {
        return {
          id: sorted.key,
          name: sorted.value,
        };
      }),

      allTags: sortedTags,
    };
  }, [value, values]);

  const handleTagAdd = useCallback(
    (newTag: SelectTag) => {
      const existingTag = allTags.some((tag) => tag.key === newTag.id);
      const newValue = value.slice();

      if (existingTag) {
        newValue.push(newTag.id);
      }

      onChange({ name, value: newValue });
    },
    [name, value, allTags, onChange]
  );

  const handleTagDelete = useCallback(
    ({ index }: { index: number }) => {
      const newValue = value.slice();
      newValue.splice(index, 1);

      onChange({
        name,
        value: newValue,
      });
    },
    [name, value, onChange]
  );

  return (
    <TagInput
      {...otherProps}
      name={name}
      tags={tags}
      tagList={tagList}
      onTagAdd={handleTagAdd}
      onTagDelete={handleTagDelete}
    />
  );
}

export default TagSelectInput;
