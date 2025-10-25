import React, { useCallback } from 'react';
import { useSelect } from 'App/Select/SelectContext';
import PageToolbarButton, {
  PageToolbarButtonProps,
} from 'Components/Page/Toolbar/PageToolbarButton';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

interface SeriesIndexSelectAllButtonProps
  extends Omit<PageToolbarButtonProps, 'iconName'> {
  isSelectMode: boolean;
}

function SeriesIndexSelectAllButton(props: SeriesIndexSelectAllButtonProps) {
  const { isSelectMode, overflowComponent } = props;
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

  return isSelectMode ? (
    <PageToolbarButton
      label={allSelected ? translate('UnselectAll') : translate('SelectAll')}
      iconName={icon}
      overflowComponent={overflowComponent}
      onPress={onPress}
    />
  ) : null;
}

export default SeriesIndexSelectAllButton;
