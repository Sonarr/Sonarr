import React, { useCallback, useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import SeriesMonitoringOptionsPopoverContent from 'AddSeries/SeriesMonitoringOptionsPopoverContent';
import AppState from 'App/State/AppState';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import SpinnerButton from 'Components/Link/SpinnerButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Popover from 'Components/Tooltip/Popover';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { icons, tooltipPositions } from 'Helpers/Props';
import { updateSeriesMonitor } from 'Store/Actions/seriesActions';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import styles from './MonitoringOptionsModalContent.css';

const NO_CHANGE = 'noChange';

export interface MonitoringOptionsModalContentProps {
  seriesId: number;
  onModalClose: () => void;
}

function MonitoringOptionsModalContent({
  seriesId,
  onModalClose,
}: MonitoringOptionsModalContentProps) {
  const dispatch = useDispatch();
  const { isSaving, saveError } = useSelector(
    (state: AppState) => state.series
  );

  const [monitor, setMonitor] = useState(NO_CHANGE);
  const wasSaving = usePrevious(isSaving);

  const handleMonitorChange = useCallback(({ value }: InputChanged<string>) => {
    setMonitor(value);
  }, []);

  const handleSavePress = useCallback(() => {
    if (monitor === NO_CHANGE) {
      return;
    }

    dispatch(
      updateSeriesMonitor({
        seriesIds: [seriesId],
        monitor,
        shouldFetchEpisodesAfterUpdate: true,
      })
    );
  }, [monitor, seriesId, dispatch]);

  useEffect(() => {
    if (!isSaving && wasSaving && !saveError) {
      onModalClose();
    }
  }, [isSaving, wasSaving, saveError, onModalClose]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('MonitorSeries')}</ModalHeader>

      <ModalBody>
        <Form>
          <FormGroup>
            <FormLabel>
              {translate('Monitoring')}

              <Popover
                anchor={<Icon className={styles.labelIcon} name={icons.INFO} />}
                title={translate('MonitoringOptions')}
                body={<SeriesMonitoringOptionsPopoverContent />}
                position={tooltipPositions.RIGHT}
              />
            </FormLabel>

            <FormInputGroup
              type="monitorEpisodesSelect"
              name="monitor"
              value={monitor}
              includeNoChange={true}
              onChange={handleMonitorChange}
            />
          </FormGroup>
        </Form>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <SpinnerButton isSpinning={isSaving} onPress={handleSavePress}>
          {translate('Save')}
        </SpinnerButton>
      </ModalFooter>
    </ModalContent>
  );
}

export default MonitoringOptionsModalContent;
