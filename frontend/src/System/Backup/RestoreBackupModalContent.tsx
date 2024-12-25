import React, { useCallback, useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { Error } from 'App/State/AppSectionState';
import AppState from 'App/State/AppState';
import TextInput from 'Components/Form/TextInput';
import Icon, { IconName, IconProps } from 'Components/Icon';
import Button from 'Components/Link/Button';
import SpinnerButton from 'Components/Link/SpinnerButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { icons, kinds } from 'Helpers/Props';
import { restart, restoreBackup } from 'Store/Actions/systemActions';
import { FileInputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import styles from './RestoreBackupModalContent.css';

function getErrorMessage(error: Error) {
  if (
    !error ||
    !error.responseJSON ||
    !('message' in error.responseJSON) ||
    !error.responseJSON.message
  ) {
    return translate('ErrorRestoringBackup');
  }

  return error.responseJSON.message;
}

function getStepIconProps(
  isExecuting: boolean,
  hasExecuted: boolean,
  error?: Error
): {
  name: IconName;
  kind?: IconProps['kind'];
  title?: string;
  isSpinning?: boolean;
} {
  if (isExecuting) {
    return {
      name: icons.SPINNER,
      isSpinning: true,
    };
  }

  if (hasExecuted) {
    return {
      name: icons.CHECK,
      kind: 'success',
    };
  }

  if (error) {
    return {
      name: icons.FATAL,
      kind: 'danger',
      title: getErrorMessage(error),
    };
  }

  return {
    name: icons.PENDING,
  };
}

export interface RestoreBackupModalContentProps {
  id?: number;
  name?: string;
  onModalClose: () => void;
}

function RestoreBackupModalContent({
  id,
  name,
  onModalClose,
}: RestoreBackupModalContentProps) {
  const { isRestoring, restoreError } = useSelector(
    (state: AppState) => state.system.backups
  );

  const { isRestarting } = useSelector((state: AppState) => state.app);

  const dispatch = useDispatch();
  const [path, setPath] = useState('');
  const [file, setFile] = useState<File | null>(null);
  const [isRestored, setIsRestored] = useState(false);
  const [isRestarted, setIsRestarted] = useState(false);
  const [isReloading, setIsReloading] = useState(false);
  const wasRestoring = usePrevious(isRestoring);
  const wasRestarting = usePrevious(isRestarting);

  const isRestoreDisabled =
    (!id && !path) || isRestoring || isRestarting || isReloading;

  const handlePathChange = useCallback(({ value, files }: FileInputChanged) => {
    if (!files?.length) {
      return;
    }

    setPath(value);
    setFile(files[0]);
  }, []);

  const handleRestorePress = useCallback(() => {
    dispatch(restoreBackup({ id, file }));
  }, [id, file, dispatch]);

  useEffect(() => {
    if (wasRestoring && !isRestoring && !restoreError) {
      setIsRestored(true);
      dispatch(restart());
    }
  }, [isRestoring, wasRestoring, restoreError, dispatch]);

  useEffect(() => {
    if (wasRestarting && !isRestarting) {
      setIsRestarted(true);
      setIsReloading(true);
      window.location.reload();
    }
  }, [isRestarting, wasRestarting, dispatch]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>Restore Backup</ModalHeader>

      <ModalBody>
        {id && name ? (
          translate('WouldYouLikeToRestoreBackup', {
            name,
          })
        ) : (
          <TextInput
            type="file"
            name="path"
            value={path}
            onChange={handlePathChange}
          />
        )}

        <div className={styles.steps}>
          <div className={styles.step}>
            <div className={styles.stepState}>
              <Icon
                size={20}
                {...getStepIconProps(isRestoring, isRestored, restoreError)}
              />
            </div>

            <div>{translate('Restore')}</div>
          </div>

          <div className={styles.step}>
            <div className={styles.stepState}>
              <Icon
                size={20}
                {...getStepIconProps(isRestarting, isRestarted)}
              />
            </div>

            <div>{translate('Restart')}</div>
          </div>

          <div className={styles.step}>
            <div className={styles.stepState}>
              <Icon size={20} {...getStepIconProps(isReloading, false)} />
            </div>

            <div>{translate('Reload')}</div>
          </div>
        </div>
      </ModalBody>

      <ModalFooter>
        <div className={styles.additionalInfo}>
          {translate('RestartReloadNote')}
        </div>

        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <SpinnerButton
          kind={kinds.WARNING}
          isDisabled={isRestoreDisabled}
          isSpinning={isRestoring}
          onPress={handleRestorePress}
        >
          {translate('Restore')}
        </SpinnerButton>
      </ModalFooter>
    </ModalContent>
  );
}

export default RestoreBackupModalContent;
