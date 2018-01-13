import PropTypes from 'prop-types';
import React from 'react';
import { icons } from 'Helpers/Props';
import SpinnerIcon from 'Components/SpinnerIcon';
import styles from './SeriesEditorFooterLabel.css';

function SeriesEditorFooterLabel(props) {
  const {
    className,
    label,
    isSaving
  } = props;

  return (
    <div className={className}>
      {label}

      {
        isSaving &&
          <SpinnerIcon
            className={styles.savingIcon}
            name={icons.SPINNER}
            isSpinning={true}
          />
      }
    </div>
  );
}

SeriesEditorFooterLabel.propTypes = {
  className: PropTypes.string.isRequired,
  label: PropTypes.string.isRequired,
  isSaving: PropTypes.bool.isRequired
};

SeriesEditorFooterLabel.defaultProps = {
  className: styles.label
};

export default SeriesEditorFooterLabel;
