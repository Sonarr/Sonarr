import React, { useCallback, useState } from 'react';
import Form from 'Components/Form/Form';
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
import styles from './ChangeMonitoringModalContent.css';

const NO_CHANGE = 'noChange';

interface ChangeMonitoringModalContentProps {
  seriesIds: number[];
  saveError?: object;
  onSavePress(monitor: string): void;
  onModalClose(): void;
}

function ChangeMonitoringModalContent(
  props: ChangeMonitoringModalContentProps
) {
  const { seriesIds, onSavePress, onModalClose, ...otherProps } = props;

  const [monitor, setMonitor] = useState(NO_CHANGE);

  const onInputChange = useCallback(
    ({ value }) => {
      setMonitor(value);
    },
    [setMonitor]
  );

  const onSavePressWrapper = useCallback(() => {
    onSavePress(monitor);
  }, [monitor, onSavePress]);

  const selectedCount = seriesIds.length;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('Monitor Series')}</ModalHeader>

      <ModalBody>
        <Form {...otherProps}>
          <FormGroup>
            <FormLabel>{translate('Monitoring')}</FormLabel>

            <FormInputGroup
              type={inputTypes.MONITOR_EPISODES_SELECT}
              name="monitor"
              value={monitor}
              includeNoChange={true}
              onChange={onInputChange}
            />
          </FormGroup>
        </Form>
      </ModalBody>

      <ModalFooter className={styles.modalFooter}>
        <div className={styles.selected}>
          {translate('{count} series selected', { count: selectedCount })}
        </div>

        <div>
          <Button onPress={onModalClose}>{translate('Cancel')}</Button>

          <Button onPress={onSavePressWrapper}>{translate('Save')}</Button>
        </div>
      </ModalFooter>
    </ModalContent>
  );
}

export default ChangeMonitoringModalContent;
