import PropTypes from 'prop-types';
import React from 'react';
import formatBytes from 'Utilities/Number/formatBytes';

function QualityDefinitionLimits(props) {
  const {
    bytes,
    message
  } = props;

  if (!bytes) {
    return <div>{message}</div>;
  }

  const thirty = formatBytes(bytes * 30, 2);
  const fourtyFive = formatBytes(bytes * 30, 2);
  const sixty = formatBytes(bytes * 30, 2);

  return (
    <div>
      <div>30 Minutes: {thirty}</div>
      <div>45 Minutes: {fourtyFive}</div>
      <div>60 Minutes: {sixty}</div>
    </div>
  );
}

QualityDefinitionLimits.propTypes = {
  bytes: PropTypes.number.isRequired,
  message: PropTypes.string.isRequired
};

export default QualityDefinitionLimits;
