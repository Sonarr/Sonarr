import React, { useCallback, useState } from 'react';
import { useSelect } from 'App/Select/SelectContext';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import { EnhancedSelectInputValue } from 'Components/Form/Select/EnhancedSelectInput';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes } from 'Helpers/Props';
import MoveSeriesModal from 'Series/MoveSeries/MoveSeriesModal';
import { InputChanged } from 'typings/inputs';
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

export interface EditSeriesModalContentProps {
  onSavePress(payload: object): void;
  onModalClose(): void;
}

const NO_CHANGE = 'noChange';

const monitoredOptions: EnhancedSelectInputValue<string>[] = [
  {
    key: NO_CHANGE,
    get value() {
      return translate('NoChange');
    },
    isDisabled: true,
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

const seasonFolderOptions: EnhancedSelectInputValue<string>[] = [
  {
    key: NO_CHANGE,
    get value() {
      return translate('NoChange');
    },
    isDisabled: true,
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
  const { onSavePress, onModalClose } = props;

  const [monitored, setMonitored] = useState(NO_CHANGE);
  const [monitorNewItems, setMonitorNewItems] = useState(NO_CHANGE);
  const [qualityProfileId, setQualityProfileId] = useState<string | number>(
    NO_CHANGE
  );
  const [seriesType, setSeriesType] = useState(NO_CHANGE);
  const [seasonFolder, setSeasonFolder] = useState(NO_CHANGE);
  const [rootFolderPath, setRootFolderPath] = useState(NO_CHANGE);
  const [isConfirmMoveModalOpen, setIsConfirmMoveModalOpen] = useState(false);
  const { selectedCount } = useSelect();

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
    ({ name, value }: InputChanged) => {
      switch (name) {
        case 'monitored':
          setMonitored(value as string);
          break;
        case 'monitorNewItems':
          setMonitorNewItems(value as string);
          break;
        case 'qualityProfileId':
          setQualityProfileId(value as string);
          break;
        case 'seriesType':
          setSeriesType(value as string);
          break;
        case 'seasonFolder':
          setSeasonFolder(value as string);
          break;
        case 'rootFolderPath':
          setRootFolderPath(value as string);
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

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('EditSelectedSeries')}</ModalHeader>
      <ModalBody>
        <FormRow>
          <FormLabel>{translate('Monitored')}</FormLabel>

          <FormInput
            type={inputTypes.SELECT}
            name="monitored"
            value={monitored}
            values={monitoredOptions}
            onChange={onInputChange}
          />
        </FormRow>

        <FormRow>
          <FormLabel>{translate('MonitorNewItems')}</FormLabel>

          <FormInput
            type={inputTypes.MONITOR_NEW_ITEMS_SELECT}
            name="monitorNewItems"
            value={monitorNewItems}
            includeNoChange={true}
            includeNoChangeDisabled={false}
            onChange={onInputChange}
          />
        </FormRow>

        <FormRow>
          <FormLabel>{translate('QualityProfile')}</FormLabel>

          <FormInput
            type={inputTypes.QUALITY_PROFILE_SELECT}
            name="qualityProfileId"
            value={qualityProfileId}
            includeNoChange={true}
            includeNoChangeDisabled={false}
            onChange={onInputChange}
          />
        </FormRow>

        <FormRow>
          <FormLabel>{translate('SeriesType')}</FormLabel>
          <FormInputHelpText text={translate('SeriesTypesHelpText')} />
          <FormInput
            type={inputTypes.SERIES_TYPE_SELECT}
            name="seriesType"
            value={seriesType}
            includeNoChange={true}
            includeNoChangeDisabled={false}
            onChange={onInputChange}
          />
        </FormRow>

        <FormRow>
          <FormLabel>{translate('SeasonFolder')}</FormLabel>

          <FormInput
            type={inputTypes.SELECT}
            name="seasonFolder"
            value={seasonFolder}
            values={seasonFolderOptions}
            onChange={onInputChange}
          />
        </FormRow>

        <FormRow>
          <FormLabel>{translate('RootFolder')}</FormLabel>
          <FormInputHelpText text={translate('SeriesEditRootFolderHelpText')} />
          <FormInput
            type={inputTypes.ROOT_FOLDER_SELECT}
            name="rootFolderPath"
            value={rootFolderPath}
            includeNoChange={true}
            includeNoChangeDisabled={false}
            selectedValueOptions={{ includeFreeSpace: false }}
            onChange={onInputChange}
          />
        </FormRow>
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
