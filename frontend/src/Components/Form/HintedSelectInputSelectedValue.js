import PropTypes from 'prop-types';
import React from 'react';
import EnhancedSelectInputSelectedValue from './EnhancedSelectInputSelectedValue';
import styles from './HintedSelectInputSelectedValue.css';

function HintedSelectInputSelectedValue(props) {
  const {
    value,
    hint,
    includeHint,
    ...otherProps
  } = props;

  return (
    <EnhancedSelectInputSelectedValue
      className={styles.selectedValue}
      {...otherProps}
    >
      <div className={styles.valueText}>
        {value}
      </div>

      {
        hint != null && includeHint &&
          <div className={styles.hintText}>
            {hint}
          </div>
      }
    </EnhancedSelectInputSelectedValue>
  );
}

HintedSelectInputSelectedValue.propTypes = {
  value: PropTypes.string,
  hint: PropTypes.string,
  includeHint: PropTypes.bool.isRequired
};

HintedSelectInputSelectedValue.defaultProps = {
  includeHint: true
};

export default HintedSelectInputSelectedValue;
