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
  const { label, iconName, isSelectMode, onPress } = props;
  const { unselectAll } = useSelect();

  const onPressWrapper = useCallback(() => {
    if (isSelectMode) {
      unselectAll();
    }

    onPress();
  }, [isSelectMode, onPress, unselectAll]);

  return (
    <PageToolbarButton
      label={label}
      iconName={iconName}
      onPress={onPressWrapper}
    />
  );
}

export default SeriesIndexSelectModeButton;
