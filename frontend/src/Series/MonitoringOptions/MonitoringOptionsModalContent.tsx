import React, { useCallback, useEffect, useState } from 'react';
import SeriesMonitoringOptionsPopoverContent from 'AddSeries/SeriesMonitoringOptionsPopoverContent';
import Alert from 'Components/Alert';
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
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import { useUpdateSeriesMonitor } from 'Series/useSeries';
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
  const {
    updateSeriesMonitor,
    isUpdatingSeriesMonitor,
    updateSeriesMonitorError,
  } = useUpdateSeriesMonitor(true);

  const [monitor, setMonitor] = useState(NO_CHANGE);
  const wasSaving = usePrevious(isUpdatingSeriesMonitor);

  const handleMonitorChange = useCallback(({ value }: InputChanged<string>) => {
    setMonitor(value);
  }, []);

  const handleSavePress = useCallback(() => {
    if (monitor === NO_CHANGE) {
      return;
    }

    updateSeriesMonitor({
      series: [
        {
          id: seriesId,
        },
      ],
      monitoringOptions: { monitor },
    });
  }, [monitor, seriesId, updateSeriesMonitor]);

  useEffect(() => {
    if (!isUpdatingSeriesMonitor && wasSaving && !updateSeriesMonitorError) {
      onModalClose();
    }
  }, [
    isUpdatingSeriesMonitor,
    wasSaving,
    updateSeriesMonitorError,
    onModalClose,
  ]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('MonitorEpisodes')}</ModalHeader>

      <ModalBody>
        <Alert kind={kinds.INFO}>
          <div>{translate('MonitorEpisodesModalInfo')}</div>
        </Alert>
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

        <SpinnerButton
          isSpinning={isUpdatingSeriesMonitor}
          onPress={handleSavePress}
        >
          {translate('Save')}
        </SpinnerButton>
      </ModalFooter>
    </ModalContent>
  );
}

export default MonitoringOptionsModalContent;
