import PropTypes from 'prop-types';
import React from 'react';

export default function TagInUse(props) {
  const {
    label,
    labelPlural,
    count
  } = props;

  if (count === 0) {
    return null;
  }

  if (count > 1 && labelPlural) {
    return (
      <div>
        {count} {labelPlural.toLowerCase()}
      </div>
    );
  }

  return (
    <div>
      {count} {label.toLowerCase()}
    </div>
  );
}

TagInUse.propTypes = {
  label: PropTypes.string.isRequired,
  labelPlural: PropTypes.string,
  count: PropTypes.number.isRequired
};
