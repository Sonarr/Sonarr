import React, { useCallback } from 'react';
import { useSelect } from 'App/SelectContext';
import PageToolbarButton, {
  PageToolbarButtonProps,
} from 'Components/Page/Toolbar/PageToolbarButton';

interface SeriesIndexSelectModeButtonProps extends PageToolbarButtonProps {
  isSelectMode: boolean;
  onPress: () => void;
}

function SeriesIndexSelectModeButton(props: SeriesIndexSelectModeButtonProps) {
  const { label, iconName, isSelectMode, overflowComponent, onPress } = props;
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
    <PageToolbarButton
      label={label}
      iconName={iconName}
      overflowComponent={overflowComponent}
      onPress={onPressWrapper}
    />
  );
}

export default SeriesIndexSelectModeButton;
