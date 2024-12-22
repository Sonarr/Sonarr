import React from 'react';
import { TagBase } from 'Components/Form/Tag/TagInput';
import TagInputTag, { TagInputTagProps } from 'Components/Form/Tag/TagInputTag';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './FilterBuilderRowValueTag.css';

interface FilterBuilderRowValueTagProps extends TagInputTagProps<TagBase> {
  isLastTag: boolean;
}

function FilterBuilderRowValueTag({
  isLastTag,
  ...otherProps
}: FilterBuilderRowValueTagProps) {
  return (
    <div className={styles.tag}>
      <TagInputTag {...otherProps} kind={kinds.DEFAULT} />

      {isLastTag ? null : <div className={styles.or}>{translate('Or')}</div>}
    </div>
  );
}

export default FilterBuilderRowValueTag;
