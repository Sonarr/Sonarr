import { IconDefinition } from '@fortawesome/fontawesome-common-types';
import React, { useCallback } from 'react';
import { useSelect } from 'App/SelectContext';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';

interface SeriesIndexSelectModeButtonProps {
  label: string;
  iconName: IconDefinition;
  isSelectMode: boolean;
  overflowComponent: React.FunctionComponent<never>;
  onPress: () => void;
}

function SeriesIndexSelectModeButton(props: SeriesIndexSelectModeButtonProps) {
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
    <PageToolbarButton
      label={label}
      iconName={iconName}
      onPress={onPressWrapper}
    />
  );
}

export default SeriesIndexSelectModeButton;
