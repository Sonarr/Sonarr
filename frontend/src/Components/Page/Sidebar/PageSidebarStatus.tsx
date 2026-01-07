import React from 'react';
import Label, { LabelProps } from 'Components/Label';
import { Kind } from 'Helpers/Props/kinds';

interface PageSidebarStatusProps extends Omit<LabelProps, 'children' | 'kind'> {
  count?: number;
  errors?: boolean;
  warnings?: boolean;
}

function PageSidebarStatus({
  count,
  errors,
  warnings,
  ...otherProps
}: PageSidebarStatusProps) {
  if (!count) {
    return null;
  }

  let kind: Kind = 'info';

  if (errors) {
    kind = 'danger';
  } else if (warnings) {
    kind = 'warning';
  }

  return (
    <Label {...otherProps} kind={kind} size="medium">
      {count}
    </Label>
  );
}

export default PageSidebarStatus;
