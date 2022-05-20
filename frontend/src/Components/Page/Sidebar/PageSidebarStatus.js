import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';
import { kinds, sizes } from 'Helpers/Props';

function PageSidebarStatus({ count, errors, warnings }) {
  if (!count) {
    return null;
  }

  let kind = kinds.INFO;

  if (errors) {
    kind = kinds.DANGER;
  } else if (warnings) {
    kind = kinds.WARNING;
  }

  return (
    <Label
      kind={kind}
      size={sizes.MEDIUM}
    >
      {count}
    </Label>
  );
}

PageSidebarStatus.propTypes = {
  count: PropTypes.number,
  errors: PropTypes.bool,
  warnings: PropTypes.bool
};

export default PageSidebarStatus;
