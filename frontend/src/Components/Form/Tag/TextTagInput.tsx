import React, { useCallback, useMemo } from 'react';
import { InputChanged } from 'typings/inputs';
import split from 'Utilities/String/split';
import TagInput, { ReplacementTag, TagBase, TagInputProps } from './TagInput';

interface TextTag extends TagBase {
  id: string;
  name: string;
}

export interface TextTagInputProps
  extends Omit<
    TagInputProps<TextTag>,
    'tags' | 'tagList' | 'onTagAdd' | 'onTagDelete'
  > {
  name: string;
  value: string | string[];
  onChange: (change: InputChanged<string[]>) => unknown;
}

function TextTagInput({
  name,
  value,
  onChange,
  ...otherProps
}: TextTagInputProps) {
  const { tags, tagList, valueArray } = useMemo(() => {
    const tagsArray = Array.isArray(value) ? value : split(value);

    return {
      tags: tagsArray.reduce((result: TextTag[], tag) => {
        if (tag) {
          result.push({
            id: tag,
            name: tag,
          });
        }

        return result;
      }, []),
      tagList: [],
      valueArray: tagsArray,
    };
  }, [value]);

  const handleTagAdd = useCallback(
    (newTag: TextTag) => {
      // Split and trim tags before adding them to the list, this will
      // cleanse tags pasted in that had commas and spaces which leads
      // to oddities with restrictions (as an example).

      const newValue = [...valueArray];
      const newTags = newTag.name.startsWith('/')
        ? [newTag.name]
        : split(newTag.name);

      newTags.forEach((newTag) => {
        const newTagValue = newTag.trim();

        if (newTagValue) {
          newValue.push(newTagValue);
        }
      });

      onChange({ name, value: newValue });
    },
    [name, valueArray, onChange]
  );

  const handleTagDelete = useCallback(
    ({ index }: { index: number }) => {
      const newValue = [...valueArray];
      newValue.splice(index, 1);

      onChange({
        name,
        value: newValue,
      });
    },
    [name, valueArray, onChange]
  );

  const handleTagReplace = useCallback(
    (tagToReplace: ReplacementTag<TextTag>, newTagName: string) => {
      const newValue = [...valueArray];
      newValue.splice(tagToReplace.index, 1);

      const newTagValue = newTagName.trim();

      if (newTagValue) {
        newValue.push(newTagValue);
      }

      onChange({ name, value: newValue });
    },
    [name, valueArray, onChange]
  );

  return (
    <TagInput
      {...otherProps}
      name={name}
      delimiters={['Tab', 'Enter', ',']}
      tags={tags}
      tagList={tagList}
      onTagAdd={handleTagAdd}
      onTagDelete={handleTagDelete}
      onTagReplace={handleTagReplace}
    />
  );
}

export default TextTagInput;
