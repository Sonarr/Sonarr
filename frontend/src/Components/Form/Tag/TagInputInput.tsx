import React, { MouseEvent, Ref, useCallback } from 'react';
import { Kind } from 'Helpers/Props/kinds';
import { TagBase } from './TagInput';
import { TagInputTagProps } from './TagInputTag';
import styles from './TagInputInput.css';

interface TagInputInputProps<T extends TagBase> {
  forwardedRef?: Ref<HTMLDivElement>;
  className?: string;
  tags: TagBase[];
  inputProps: object;
  kind: Kind;
  isFocused: boolean;
  canEdit: boolean;
  tagComponent: React.ElementType;
  onTagDelete: TagInputTagProps<T>['onDelete'];
  onTagEdit: TagInputTagProps<T>['onEdit'];
  onInputContainerPress: () => void;
}

function TagInputInput<T extends TagBase>(props: TagInputInputProps<T>) {
  const {
    forwardedRef,
    className = styles.inputContainer,
    tags,
    inputProps,
    kind,
    isFocused,
    canEdit,
    tagComponent: TagComponent,
    onTagDelete,
    onTagEdit,
    onInputContainerPress,
  } = props;

  const handleMouseDown = useCallback(
    (event: MouseEvent<HTMLDivElement>) => {
      event.preventDefault();

      if (isFocused) {
        return;
      }

      onInputContainerPress();
    },
    [isFocused, onInputContainerPress]
  );

  return (
    <div ref={forwardedRef} className={className} onMouseDown={handleMouseDown}>
      {tags.map((tag, index) => {
        return (
          <TagComponent
            key={tag.id}
            index={index}
            tag={tag}
            kind={kind}
            canEdit={canEdit}
            isLastTag={index === tags.length - 1}
            onDelete={onTagDelete}
            onEdit={onTagEdit}
          />
        );
      })}

      <input {...inputProps} />
    </div>
  );
}

export default TagInputInput;
