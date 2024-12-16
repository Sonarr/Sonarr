import React, { useCallback } from 'react';
import { useSelect } from 'App/SelectContext';
import { IconName } from 'Components/Icon';
import PageToolbarOverflowMenuItem from 'Components/Page/Toolbar/PageToolbarOverflowMenuItem';

interface SeriesIndexSelectModeMenuItemProps {
  label: string;
  iconName: IconName;
  isSelectMode: boolean;
  onPress: () => void;
}

function SeriesIndexSelectModeMenuItem(
  props: SeriesIndexSelectModeMenuItemProps
) {
  const { label, iconName, isSelectMode, onPress } = props;
  const [, selectDispatch] = useSelect();

  const onPressWrapper = useCallback(() => {
    if (isSelectMode) {
      selectDispatch({
        type: 'reset',
      });
    }

    onPress();
  }, [isSelectMode, onPress, selectDispatch]);

  return (
    <PageToolbarOverflowMenuItem
      label={label}
      iconName={iconName}
      onPress={onPressWrapper}
    />
  );
}

export default SeriesIndexSelectModeMenuItem;
