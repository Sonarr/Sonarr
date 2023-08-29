import React, { useCallback } from 'react';
import { useSelect } from 'App/SelectContext';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

interface SeriesIndexSelectAllButtonProps {
  label: string;
  isSelectMode: boolean;
  overflowComponent: React.FunctionComponent<never>;
}

function SeriesIndexSelectAllButton(props: SeriesIndexSelectAllButtonProps) {
  const { isSelectMode } = props;
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
      type: allSelected ? 'unselectAll' : 'selectAll',
    });
  }, [allSelected, selectDispatch]);

  return isSelectMode ? (
    <PageToolbarButton
      label={allSelected ? translate('UnselectAll') : translate('SelectAll')}
      iconName={icon}
      onPress={onPress}
    />
  ) : null;
}

export default SeriesIndexSelectAllButton;
