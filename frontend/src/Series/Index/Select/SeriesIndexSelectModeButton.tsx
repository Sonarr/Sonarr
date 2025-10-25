import React, { useCallback } from 'react';
import { useSelect } from 'App/Select/SelectContext';
import PageToolbarButton, {
  PageToolbarButtonProps,
} from 'Components/Page/Toolbar/PageToolbarButton';

interface SeriesIndexSelectModeButtonProps extends PageToolbarButtonProps {
  isSelectMode: boolean;
  onPress: () => void;
}

function SeriesIndexSelectModeButton(props: SeriesIndexSelectModeButtonProps) {
  const { label, iconName, isSelectMode, overflowComponent, onPress } = props;
  const { reset } = useSelect();

  const onPressWrapper = useCallback(() => {
    if (isSelectMode) {
      reset();
    }

    onPress();
  }, [isSelectMode, onPress, reset]);

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
