import PropTypes from 'prop-types';
import React from 'react';
import monitorOptions from 'Utilities/Series/monitorOptions';
import SelectInput from './SelectInput';

function MonitorEpisodesSelectInput(props) {
  const {
    includeNoChange,
    includeMixed,
    ...otherProps
  } = props;

  const values = [...monitorOptions];

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
      values={values}
      {...otherProps}
    />
  );
}

MonitorEpisodesSelectInput.propTypes = {
  includeNoChange: PropTypes.bool.isRequired,
  includeMixed: PropTypes.bool.isRequired,
  onChange: PropTypes.func.isRequired
};

MonitorEpisodesSelectInput.defaultProps = {
  includeNoChange: false,
  includeMixed: false
};

export default MonitorEpisodesSelectInput;
