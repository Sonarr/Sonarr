import React from 'react';
import Icon from 'Components/Icon';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

interface SeriesMovePendingProps {
  className?: string;
  pendingPath: string;
}

function SeriesMovePending({ className, pendingPath }: SeriesMovePendingProps) {
  return (
    <Icon
      className={className}
      name={icons.PENDING}
      title={translate('SeriesMovePending', { path: pendingPath })}
    />
  );
}

export default SeriesMovePending;
