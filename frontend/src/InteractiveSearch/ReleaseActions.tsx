import React from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import { icons, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './ReleaseActions.module.css';

function getDownloadIcon(
  isGrabbing: boolean,
  isGrabbed: boolean,
  grabError?: string
) {
  if (isGrabbing) {
    return icons.SPINNER;
  } else if (isGrabbed) {
    return icons.DOWNLOADING;
  } else if (grabError) {
    return icons.DOWNLOADING;
  }

  return icons.DOWNLOAD;
}

function getDownloadKind(isGrabbed: boolean, grabError?: string) {
  if (isGrabbed) {
    return kinds.SUCCESS;
  }

  if (grabError) {
    return kinds.DANGER;
  }

  return kinds.DEFAULT;
}

function getDownloadTooltip(
  isGrabbing: boolean,
  isGrabbed: boolean,
  grabError?: string
) {
  if (isGrabbing) {
    return '';
  } else if (isGrabbed) {
    return translate('AddedToDownloadQueue');
  } else if (grabError) {
    return grabError;
  }

  return translate('AddToDownloadQueue');
}

interface ReleaseActionsProps {
  isGrabbing: boolean;
  isGrabbed: boolean;
  grabError?: string;
  onGrabPress: () => void;
  onOverridePress: () => void;
}

function ReleaseActions({
  isGrabbing,
  isGrabbed,
  grabError,
  onGrabPress,
  onOverridePress,
}: ReleaseActionsProps) {
  return (
    <div className={styles.actions}>
      <Link
        className={styles.actionButton}
        title={translate('OverrideAndAddToDownloadQueue')}
        onPress={onOverridePress}
      >
        <div className={styles.manualDownloadContent}>
          <Icon
            className={styles.interactiveIcon}
            name={icons.INTERACTIVE}
            size={12}
          />

          <Icon
            className={styles.downloadIcon}
            name={icons.CIRCLE_DOWN}
            size={10}
          />
        </div>
      </Link>

      <SpinnerIconButton
        className={styles.actionButton}
        name={getDownloadIcon(isGrabbing, isGrabbed, grabError)}
        kind={getDownloadKind(isGrabbed, grabError)}
        title={getDownloadTooltip(isGrabbing, isGrabbed, grabError)}
        isSpinning={isGrabbing}
        onPress={onGrabPress}
      />
    </div>
  );
}

export default ReleaseActions;
