import PropTypes from 'prop-types';
import React from 'react';
import SelectInput from './SelectInput';

const seriesTypeOptions = [
  { key: 'standard', value: 'Standard' },
  { key: 'daily', value: 'Daily' },
  { key: 'anime', value: 'Anime' }
];

function SeriesTypeSelectInput(props) {
  const values = [...seriesTypeOptions];

  const {
    includeNoChange,
    includeMixed
  } = props;

  if (includeNoChange) {
    values.unshift({
      key: 'noChange',
      value: 'No Change',
      disabled: true
    });
  }

  if (includeMixed) {
    values.unshift({
      key: 'mixed',
      value: '(Mixed)',
      disabled: true
    });
  }

  return (
    <SelectInput
      {...props}
      values={values}
    />
  );
}

SeriesTypeSelectInput.propTypes = {
  includeNoChange: PropTypes.bool.isRequired,
  includeMixed: PropTypes.bool.isRequired
};

SeriesTypeSelectInput.defaultProps = {
  includeNoChange: false,
  includeMixed: false
};

export default SeriesTypeSelectInput;
