import React, { useCallback } from 'react';
import { useSelect } from 'App/Select/SelectContext';
import PageToolbarOverflowMenuItem from 'Components/Page/Toolbar/PageToolbarOverflowMenuItem';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

interface SeriesIndexSelectAllMenuItemProps {
  label: string;
  isSelectMode: boolean;
}

function SeriesIndexSelectAllMenuItem(
  props: SeriesIndexSelectAllMenuItemProps
) {
  const { isSelectMode } = props;
  const { allSelected, allUnselected, selectAll, unselectAll } = useSelect();

  let iconName = icons.SQUARE_MINUS;

  if (allSelected) {
    iconName = icons.CHECK_SQUARE;
  } else if (allUnselected) {
    iconName = icons.SQUARE;
  }

  const onPressWrapper = useCallback(() => {
    if (allSelected) {
      unselectAll();
    } else {
      selectAll();
    }
  }, [allSelected, selectAll, unselectAll]);

  return isSelectMode ? (
    <PageToolbarOverflowMenuItem
      label={allSelected ? translate('UnselectAll') : translate('SelectAll')}
      iconName={iconName}
      onPress={onPressWrapper}
    />
  ) : null;
}

export default SeriesIndexSelectAllMenuItem;
