import React, { useCallback } from 'react';
import { SelectActionType, useSelect } from 'App/SelectContext';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import { icons } from 'Helpers/Props';

function SeriesIndexSelectAllButton() {
  const [selectState, selectDispatch] = useSelect();
  const { allSelected, allUnselected } = selectState;

  let icon = icons.SQUARE_MINUS;

  if (allSelected) {
    icon = icons.CHECK_SQUARE;
  } else if (allUnselected) {
    icon = icons.SQUARE;
  }

  const onPress = useCallback(() => {
    selectDispatch({
      type: allSelected
        ? SelectActionType.UnselectAll
        : SelectActionType.SelectAll,
    });
  }, [allSelected, selectDispatch]);

  return (
    <PageToolbarButton
      label={allSelected ? 'Unselect All' : 'Select All'}
      iconName={icon}
      onPress={onPress}
    />
  );
}

export default SeriesIndexSelectAllButton;
