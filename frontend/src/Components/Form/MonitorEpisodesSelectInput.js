import PropTypes from 'prop-types';
import React from 'react';
import SelectInput from './SelectInput';

const monitorOptions = [
  { key: 'all', value: 'All Episodes' },
  { key: 'future', value: 'Future Episodes' },
  { key: 'missing', value: 'Missing Episodes' },
  { key: 'existing', value: 'Existing Episodes' },
  { key: 'first', value: 'Only First Season' },
  { key: 'latest', value: 'Only Latest Season' },
  { key: 'none', value: 'None' }
];

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
