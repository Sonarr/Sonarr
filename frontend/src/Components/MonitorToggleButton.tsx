import classNames from 'classnames';
import React, { SyntheticEvent, useCallback, useMemo } from 'react';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './MonitorToggleButton.module.css';

interface MonitorToggleButtonProps {
  className?: string;
  monitored: boolean;
  size?: number;
  title?: string;
  isDisabled?: boolean;
  isSaving?: boolean;
  onPress: (value: boolean, options: { shiftKey: boolean }) => unknown;
}

function MonitorToggleButton(props: MonitorToggleButtonProps) {
  const {
    className = styles.toggleButton,
    monitored,
    isDisabled = false,
    isSaving = false,
    size,
    title: titleOverride,
    onPress,
    ...otherProps
  } = props;

  const title = useMemo(() => {
    if (titleOverride) {
      return titleOverride;
    }

    if (isDisabled) {
      return translate('ToggleMonitoredSeriesUnmonitored');
    }

    if (monitored) {
      return translate('ToggleMonitoredToUnmonitored');
    }

    return translate('ToggleUnmonitoredToMonitored');
  }, [monitored, isDisabled, titleOverride]);

  const handlePress = useCallback(
    (event: SyntheticEvent<HTMLLinkElement, MouseEvent>) => {
      const shiftKey = event.nativeEvent.shiftKey;

      onPress(!monitored, { shiftKey });
    },
    [monitored, onPress]
  );

  return (
    <SpinnerIconButton
      className={classNames(className, isDisabled && styles.isDisabled)}
      name={icons.MONITORED}
      filled={monitored}
      size={size}
      title={title}
      aria-label={title}
      isDisabled={isDisabled}
      isSpinning={isSaving}
      {...otherProps}
      onPress={handlePress}
    />
  );
}

export default MonitorToggleButton;
