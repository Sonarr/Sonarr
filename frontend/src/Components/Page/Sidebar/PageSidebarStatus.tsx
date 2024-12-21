import React from 'react';
import Label from 'Components/Label';
import { Kind } from 'Helpers/Props/kinds';

interface PageSidebarStatusProps {
  count?: number;
  errors?: boolean;
  warnings?: boolean;
}

function PageSidebarStatus({
  count,
  errors,
  warnings,
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
    <Label kind={kind} size="medium">
      {count}
    </Label>
  );
}

export default PageSidebarStatus;
