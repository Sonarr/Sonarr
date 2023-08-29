import PropTypes from 'prop-types';
import React from 'react';
import TagInputTag from 'Components/Form/TagInputTag';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './FilterBuilderRowValueTag.css';

function FilterBuilderRowValueTag(props) {
  return (
    <div
      className={styles.tag}
    >
      <TagInputTag
        kind={kinds.DEFAULT}
        {...props}
      />

      {
        props.isLastTag ?
          null :
          <div className={styles.or}>
            {translate('Or')}
          </div>
      }
    </div>
  );
}

FilterBuilderRowValueTag.propTypes = {
  isLastTag: PropTypes.bool.isRequired
};

export default FilterBuilderRowValueTag;
