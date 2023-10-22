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
import MoveSeriesModal from 'Series/MoveSeries/MoveSeriesModal';
import translate from 'Utilities/String/translate';
import styles from './EditSeriesModalContent.css';

interface SavePayload {
  monitored?: boolean;
  monitorNewItems?: string;
  qualityProfileId?: number;
  seriesType?: string;
  seasonFolder?: boolean;
  rootFolderPath?: string;
  moveFiles?: boolean;
}

interface EditSeriesModalContentProps {
  seriesIds: number[];
  onSavePress(payload: object): void;
  onModalClose(): void;
}

const NO_CHANGE = 'noChange';

const monitoredOptions = [
  {
    key: NO_CHANGE,
    get value() {
      return translate('NoChange');
    },
    disabled: true,
  },
  {
    key: 'monitored',
    get value() {
      return translate('Monitored');
    },
  },
  {
    key: 'unmonitored',
    get value() {
      return translate('Unmonitored');
    },
  },
];

const seasonFolderOptions = [
  {
    key: NO_CHANGE,
    get value() {
      return translate('NoChange');
    },
    disabled: true,
  },
  {
    key: 'yes',
    get value() {
      return translate('Yes');
    },
  },
  {
    key: 'no',
    get value() {
      return translate('No');
    },
  },
];

function EditSeriesModalContent(props: EditSeriesModalContentProps) {
  const { seriesIds, onSavePress, onModalClose } = props;

  const [monitored, setMonitored] = useState(NO_CHANGE);
  const [monitorNewItems, setMonitorNewItems] = useState(NO_CHANGE);
  const [qualityProfileId, setQualityProfileId] = useState<string | number>(
    NO_CHANGE
  );
  const [seriesType, setSeriesType] = useState(NO_CHANGE);
  const [seasonFolder, setSeasonFolder] = useState(NO_CHANGE);
  const [rootFolderPath, setRootFolderPath] = useState(NO_CHANGE);
  const [isConfirmMoveModalOpen, setIsConfirmMoveModalOpen] = useState(false);

  const save = useCallback(
    (moveFiles: boolean) => {
      let hasChanges = false;
      const payload: SavePayload = {};

      if (monitored !== NO_CHANGE) {
        hasChanges = true;
        payload.monitored = monitored === 'monitored';
      }

      if (monitorNewItems !== NO_CHANGE) {
        hasChanges = true;
        payload.monitorNewItems = monitorNewItems;
      }

      if (qualityProfileId !== NO_CHANGE) {
        hasChanges = true;
        payload.qualityProfileId = qualityProfileId as number;
      }

      if (seriesType !== NO_CHANGE) {
        hasChanges = true;
        payload.seriesType = seriesType;
      }

      if (seasonFolder !== NO_CHANGE) {
        hasChanges = true;
        payload.seasonFolder = seasonFolder === 'yes';
      }

      if (rootFolderPath !== NO_CHANGE) {
        hasChanges = true;
        payload.rootFolderPath = rootFolderPath;
        payload.moveFiles = moveFiles;
      }

      if (hasChanges) {
        onSavePress(payload);
      }

      onModalClose();
    },
    [
      monitored,
      monitorNewItems,
      qualityProfileId,
      seriesType,
      seasonFolder,
      rootFolderPath,
      onSavePress,
      onModalClose,
    ]
  );

  const onInputChange = useCallback(
    ({ name, value }: { name: string; value: string }) => {
      switch (name) {
        case 'monitored':
          setMonitored(value);
          break;
        case 'monitorNewItems':
          setMonitorNewItems(value);
          break;
        case 'qualityProfileId':
          setQualityProfileId(value);
          break;
        case 'seriesType':
          setSeriesType(value);
          break;
        case 'seasonFolder':
          setSeasonFolder(value);
          break;
        case 'rootFolderPath':
          setRootFolderPath(value);
          break;
        default:
          console.warn('EditSeriesModalContent Unknown Input');
      }
    },
    [setMonitored]
  );

  const onSavePressWrapper = useCallback(() => {
    if (rootFolderPath === NO_CHANGE) {
      save(false);
    } else {
      setIsConfirmMoveModalOpen(true);
    }
  }, [rootFolderPath, save]);

  const onCancelPress = useCallback(() => {
    setIsConfirmMoveModalOpen(false);
  }, [setIsConfirmMoveModalOpen]);

  const onDoNotMoveSeriesPress = useCallback(() => {
    setIsConfirmMoveModalOpen(false);
    save(false);
  }, [setIsConfirmMoveModalOpen, save]);

  const onMoveSeriesPress = useCallback(() => {
    setIsConfirmMoveModalOpen(false);
    save(true);
  }, [setIsConfirmMoveModalOpen, save]);

  const selectedCount = seriesIds.length;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('EditSelectedSeries')}</ModalHeader>

      <ModalBody>
        <FormGroup>
          <FormLabel>{translate('Monitored')}</FormLabel>

          <FormInputGroup
            type={inputTypes.SELECT}
            name="monitored"
            value={monitored}
            values={monitoredOptions}
            onChange={onInputChange}
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>{translate('MonitorNewItems')}</FormLabel>

          <FormInputGroup
            type={inputTypes.MONITOR_NEW_ITEMS_SELECT}
            name="monitorNewItems"
            value={monitorNewItems}
            includeNoChange={true}
            includeNoChangeDisabled={false}
            onChange={onInputChange}
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>{translate('QualityProfile')}</FormLabel>

          <FormInputGroup
            type={inputTypes.QUALITY_PROFILE_SELECT}
            name="qualityProfileId"
            value={qualityProfileId}
            includeNoChange={true}
            includeNoChangeDisabled={false}
            onChange={onInputChange}
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>{translate('SeriesType')}</FormLabel>

          <FormInputGroup
            type={inputTypes.SERIES_TYPE_SELECT}
            name="seriesType"
            value={seriesType}
            includeNoChange={true}
            includeNoChangeDisabled={false}
            helpText={translate('SeriesTypesHelpText')}
            onChange={onInputChange}
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>{translate('SeasonFolder')}</FormLabel>

          <FormInputGroup
            type={inputTypes.SELECT}
            name="seasonFolder"
            value={seasonFolder}
            values={seasonFolderOptions}
            onChange={onInputChange}
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>{translate('RootFolder')}</FormLabel>

          <FormInputGroup
            type={inputTypes.ROOT_FOLDER_SELECT}
            name="rootFolderPath"
            value={rootFolderPath}
            includeNoChange={true}
            includeNoChangeDisabled={false}
            selectedValueOptions={{ includeFreeSpace: false }}
            helpText={translate('SeriesEditRootFolderHelpText')}
            onChange={onInputChange}
          />
        </FormGroup>
      </ModalBody>

      <ModalFooter className={styles.modalFooter}>
        <div className={styles.selected}>
          {translate('CountSeriesSelected', { count: selectedCount })}
        </div>

        <div>
          <Button onPress={onModalClose}>{translate('Cancel')}</Button>

          <Button onPress={onSavePressWrapper}>
            {translate('ApplyChanges')}
          </Button>
        </div>
      </ModalFooter>

      <MoveSeriesModal
        isOpen={isConfirmMoveModalOpen}
        destinationRootFolder={rootFolderPath}
        onModalClose={onCancelPress}
        onSavePress={onDoNotMoveSeriesPress}
        onMoveSeriesPress={onMoveSeriesPress}
      />
    </ModalContent>
  );
}

export default EditSeriesModalContent;
