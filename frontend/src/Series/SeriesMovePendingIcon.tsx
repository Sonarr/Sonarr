import React from 'react';
import Icon from 'Components/Icon';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

interface SeriesMovePendingIconProps {
  className?: string;
  nextPath: string;
}

function SeriesMovePendingIcon({
  className,
  nextPath,
}: SeriesMovePendingIconProps) {
  return (
    <Icon
      className={className}
      name={icons.PENDING}
      title={translate('SeriesMovePending', { path: nextPath })}
    />
  );
}

export default SeriesMovePendingIcon;
