import React, { useCallback, useState } from 'react';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './ManageDownloadClientsEditModalContent.css';

interface SavePayload {
  enable?: boolean;
  removeCompletedDownloads?: boolean;
  removeFailedDownloads?: boolean;
  priority?: number;
}

interface ManageDownloadClientsEditModalContentProps {
  downloadClientIds: number[];
  onSavePress(payload: object): void;
  onModalClose(): void;
}

const NO_CHANGE = 'noChange';

const enableOptions = [
  { key: NO_CHANGE, value: 'No Change', disabled: true },
  { key: 'enabled', value: 'Enabled' },
  { key: 'disabled', value: 'Disabled' },
];

function ManageDownloadClientsEditModalContent(
  props: ManageDownloadClientsEditModalContentProps
) {
  const { downloadClientIds, onSavePress, onModalClose } = props;

  const [enable, setEnable] = useState(NO_CHANGE);
  const [removeCompletedDownloads, setRemoveCompletedDownloads] =
    useState(NO_CHANGE);
  const [removeFailedDownloads, setRemoveFailedDownloads] = useState(NO_CHANGE);
  const [priority, setPriority] = useState<null | string | number>(null);

  const save = useCallback(() => {
    let hasChanges = false;
    const payload: SavePayload = {};

    if (enable !== NO_CHANGE) {
      hasChanges = true;
      payload.enable = enable === 'enabled';
    }

    if (removeCompletedDownloads !== NO_CHANGE) {
      hasChanges = true;
      payload.removeCompletedDownloads = removeCompletedDownloads === 'enabled';
    }

    if (removeFailedDownloads !== NO_CHANGE) {
      hasChanges = true;
      payload.removeFailedDownloads = removeFailedDownloads === 'enabled';
    }

    if (priority !== null) {
      hasChanges = true;
      payload.priority = priority as number;
    }

    if (hasChanges) {
      onSavePress(payload);
    }

    onModalClose();
  }, [
    enable,
    priority,
    removeCompletedDownloads,
    removeFailedDownloads,
    onSavePress,
    onModalClose,
  ]);

  const onInputChange = useCallback(
    ({ name, value }: { name: string; value: string }) => {
      switch (name) {
        case 'enable':
          setEnable(value);
          break;
        case 'priority':
          setPriority(value);
          break;
        case 'removeCompletedDownloads':
          setRemoveCompletedDownloads(value);
          break;
        case 'removeFailedDownloads':
          setRemoveFailedDownloads(value);
          break;
        default:
          console.warn('EditDownloadClientsModalContent Unknown Input');
      }
    },
    []
  );

  const selectedCount = downloadClientIds.length;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('EditSelectedDownloadClients')}</ModalHeader>

      <ModalBody>
        <FormGroup>
          <FormLabel>{translate('Enabled')}</FormLabel>

          <FormInputGroup
            type={inputTypes.SELECT}
            name="enable"
            value={enable}
            values={enableOptions}
            onChange={onInputChange}
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>{translate('Priority')}</FormLabel>

          <FormInputGroup
            type={inputTypes.NUMBER}
            name="priority"
            value={priority}
            min={1}
            max={50}
            onChange={onInputChange}
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>{translate('RemoveCompletedDownloads')}</FormLabel>

          <FormInputGroup
            type={inputTypes.SELECT}
            name="removeCompletedDownloads"
            value={removeCompletedDownloads}
            values={enableOptions}
            onChange={onInputChange}
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>{translate('RemoveFailedDownloads')}</FormLabel>

          <FormInputGroup
            type={inputTypes.SELECT}
            name="removeFailedDownloads"
            value={removeFailedDownloads}
            values={enableOptions}
            onChange={onInputChange}
          />
        </FormGroup>
      </ModalBody>

      <ModalFooter className={styles.modalFooter}>
        <div className={styles.selected}>
          {translate('{count} download clients selected', {
            count: selectedCount,
          })}
        </div>

        <div>
          <Button onPress={onModalClose}>{translate('Cancel')}</Button>

          <Button onPress={save}>{translate('Apply Changes')}</Button>
        </div>
      </ModalFooter>
    </ModalContent>
  );
}

export default ManageDownloadClientsEditModalContent;
