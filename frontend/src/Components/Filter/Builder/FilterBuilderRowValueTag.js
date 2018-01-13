import PropTypes from 'prop-types';
import React from 'react';
import { kinds } from 'Helpers/Props';
import TagInputTag from 'Components/Form/TagInputTag';
import styles from './FilterBuilderRowValueTag.css';

function FilterBuilderRowValueTag(props) {
  return (
    <span
      className={styles.tag}
    >
      <TagInputTag
        kind={kinds.DEFAULT}
        {...props}
      />

      {
        !props.isLastTag &&
          <span className={styles.or}>
            or
          </span>
      }
    </span>
  );
}

FilterBuilderRowValueTag.propTypes = {
  isLastTag: PropTypes.bool.isRequired
};

export default FilterBuilderRowValueTag;
