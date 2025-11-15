import React, { useCallback, useEffect, useMemo } from 'react';
import { Tag, useAddTag, useSortedTagList } from 'Tags/useTags';
import { InputChanged } from 'typings/inputs';
import { useFormInputGroup } from '../FormInputGroupContext';
import TagInput, { TagBase, TagInputProps } from './TagInput';

interface SeriesTag extends TagBase {
  id: number;
  name: string;
}

export interface SeriesTagInputProps<V>
  extends Omit<
    TagInputProps<SeriesTag>,
    'tags' | 'tagList' | 'onTagAdd' | 'onTagDelete' | 'onChange'
  > {
  name: string;
  value: V;
  onChange: (change: InputChanged<V>) => void;
}

function useSeriesTags(tags: number[]) {
  const sortedTags = useSortedTagList();
  const filteredTagList = sortedTags.filter((tag) => !tags.includes(tag.id));

  return {
    tags: tags.reduce((acc: SeriesTag[], tag) => {
      const matchingTag = sortedTags.find((t) => t.id === tag);

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
}

export default function SeriesTagInput<V extends number | number[]>({
  name,
  value,
  onChange,
  ...otherProps
}: SeriesTagInputProps<V>) {
  const formInputActions = useFormInputGroup();
  const isArray = Array.isArray(value);

  const arrayValue = useMemo(() => {
    if (isArray) {
      return value as number[];
    }

    return value === 0 ? [] : [value as number];
  }, [isArray, value]);

  const { tags, tagList, allTags } = useSeriesTags(arrayValue);

  const handleTagCreated = useCallback(
    (tag: Tag) => {
      if (isArray) {
        onChange({ name, value: [...value, tag.id] as V });
      } else {
        onChange({
          name,
          value: tag.id as V,
        });
      }
    },
    [name, value, isArray, onChange]
  );

  const { addTag, addTagError } = useAddTag(handleTagCreated);

  const handleTagAdd = useCallback(
    (newTag: SeriesTag) => {
      if (newTag.id) {
        if (isArray) {
          onChange({ name, value: [...value, newTag.id] as V });
        } else {
          onChange({ name, value: newTag.id as V });
        }

        return;
      }

      const existingTag = allTags.some((t) => t.label === newTag.name);

      if (!existingTag) {
        addTag({
          label: newTag.name,
        });
      }
    },
    [name, value, isArray, allTags, onChange, addTag]
  );

  const handleTagDelete = useCallback(
    ({ index }: { index: number }) => {
      if (isArray) {
        const newValue = value.slice();
        newValue.splice(index, 1);

        onChange({ name, value: newValue as V });
      } else {
        onChange({ name, value: 0 as V });
      }
    },
    [name, value, isArray, onChange]
  );

  useEffect(() => {
    formInputActions?.setClientErrors(addTagError?.errors ?? []);
    formInputActions?.setClientWarnings(addTagError?.warnings ?? []);
  }, [addTagError, formInputActions]);

  useEffect(() => {
    console.info('\x1b[36m[MarkTest] formInputActions has changed\x1b[0m');
  }, [formInputActions]);

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
