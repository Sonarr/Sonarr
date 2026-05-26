import React, { useCallback } from 'react';
import { useSelect } from 'App/Select/SelectContext';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

function SeriesIndexSelectAllButton() {
  const { allSelected, allUnselected, selectAll, unselectAll } = useSelect();

  let icon = icons.SQUARE_MINUS;

  if (allSelected) {
    icon = icons.CHECK_SQUARE;
  } else if (allUnselected) {
    icon = icons.SQUARE;
  }

  const onPress = useCallback(() => {
    if (allSelected) {
      unselectAll();
    } else {
      selectAll();
    }
  }, [allSelected, selectAll, unselectAll]);

  return (
    <PageToolbarButton
      label={allSelected ? translate('UnselectAll') : translate('SelectAll')}
      iconName={icon}
      onPress={onPress}
    />
  );
}

export default SeriesIndexSelectAllButton;
