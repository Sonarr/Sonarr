import PropTypes from 'prop-types';
import React from 'react';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';

function QualityDefinitionLimits(props) {
  const {
    bytes,
    message
  } = props;

  if (!bytes) {
    return <div>{message}</div>;
  }

  const thirty = formatBytes(bytes * 30);
  const fortyFive = formatBytes(bytes * 45);
  const sixty = formatBytes(bytes * 60);

  return (
    <div>
      <div>
        {translate('MinutesThirty', { thirty })}
      </div>
      <div>
        {translate('MinutesFortyFive', { fortyFive })}
      </div>
      <div>
        {translate('MinutesSixty', { sixty })}
      </div>
    </div>
  );
}

QualityDefinitionLimits.propTypes = {
  bytes: PropTypes.number,
  message: PropTypes.string.isRequired
};

export default QualityDefinitionLimits;
