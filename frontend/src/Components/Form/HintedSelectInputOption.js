import PropTypes from 'prop-types';
import React from 'react';
import classNames from 'classnames';
import EnhancedSelectInputOption from './EnhancedSelectInputOption';
import styles from './HintedSelectInputOption.css';

function HintedSelectInputOption(props) {
  const {
    value,
    hint,
    isMobile,
    ...otherProps
  } = props;

  return (
    <EnhancedSelectInputOption
      isMobile={isMobile}
      {...otherProps}
    >
      <div className={classNames(
        styles.optionText,
        isMobile && styles.isMobile
      )}
      >
        <div>{value}</div>

        {
          hint != null &&
            <div className={styles.hintText}>
              {hint}
            </div>
        }
      </div>
    </EnhancedSelectInputOption>
  );
}

HintedSelectInputOption.propTypes = {
  value: PropTypes.string.isRequired,
  hint: PropTypes.node,
  isMobile: PropTypes.bool.isRequired
};

export default HintedSelectInputOption;
