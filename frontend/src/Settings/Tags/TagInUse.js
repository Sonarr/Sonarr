import PropTypes from 'prop-types';
import React from 'react';

export default function TagInUse(props) {
  const {
    label,
    count,
    shouldPluralize = true
  } = props;

  if (count === 0) {
    return null;
  }

  if (count > 1 && shouldPluralize) {
    return (
      <div>
        {count} {label}s
      </div>
    );
  }

  return (
    <div>
      {count} {label}
    </div>
  );
}

TagInUse.propTypes = {
  label: PropTypes.string.isRequired,
  count: PropTypes.number.isRequired,
  shouldPluralize: PropTypes.bool
};
