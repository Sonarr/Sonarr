import React, { useCallback, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import SeriesMonitorNewItemsOptionsPopoverContent from 'AddSeries/SeriesMonitorNewItemsOptionsPopoverContent';
import AppState from 'App/State/AppState';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputButton from 'Components/Form/FormInputButton';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Popover from 'Components/Tooltip/Popover';
import { icons, inputTypes, kinds, tooltipPositions } from 'Helpers/Props';
import MoveSeriesModal from 'Series/MoveSeries/MoveSeriesModal';
import useSeries from 'Series/useSeries';
import { saveSeries, setSeriesValue } from 'Store/Actions/seriesActions';
import selectSettings from 'Store/Selectors/selectSettings';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import RootFolderModal from './RootFolder/RootFolderModal';
import { RootFolderUpdated } from './RootFolder/RootFolderModalContent';
import styles from './EditSeriesModalContent.css';

export interface EditSeriesModalContentProps {
  seriesId: number;
  onModalClose: () => void;
  onDeleteSeriesPress: () => void;
}
function EditSeriesModalContent({
  seriesId,
  onModalClose,
  onDeleteSeriesPress,
}: EditSeriesModalContentProps) {
  const dispatch = useDispatch();
  const {
    title,
    monitored,
    monitorNewItems,
    seasonFolder,
    qualityProfileId,
    seriesType,
    path,
    tags,
    rootFolderPath: initialRootFolderPath,
  } = useSeries(seriesId)!;

  const { isSaving, saveError, pendingChanges } = useSelector(
    (state: AppState) => state.series
  );

  const [isRootFolderModalOpen, setIsRootFolderModalOpen] = useState(false);

  const [rootFolderPath, setRootFolderPath] = useState(initialRootFolderPath);

  const isPathChanging = pendingChanges.path && path !== pendingChanges.path;

  const [isConfirmMoveModalOpen, setIsConfirmMoveModalOpen] = useState(false);

  const { settings, ...otherSettings } = useMemo(() => {
    return selectSettings(
      {
        monitored,
        monitorNewItems,
        seasonFolder,
        qualityProfileId,
        seriesType,
        path,
        tags,
      },
      pendingChanges,
      saveError
    );
  }, [
    monitored,
    monitorNewItems,
    seasonFolder,
    qualityProfileId,
    seriesType,
    path,
    tags,
    pendingChanges,
    saveError,
  ]);

  const handleInputChange = useCallback(
    ({ name, value }: InputChanged) => {
      // @ts-expect-error actions aren't typed
      dispatch(setSeriesValue({ name, value }));
    },
    [dispatch]
  );

  const handleRootFolderPress = useCallback(() => {
    setIsRootFolderModalOpen(true);
  }, []);

  const handleRootFolderModalClose = useCallback(() => {
    setIsRootFolderModalOpen(false);
  }, []);

  const handleRootFolderChange = useCallback(
    ({
      path: newPath,
      rootFolderPath: newRootFolderPath,
    }: RootFolderUpdated) => {
      setIsRootFolderModalOpen(false);
      setRootFolderPath(newRootFolderPath);
      handleInputChange({ name: 'path', value: newPath });
    },
    [handleInputChange]
  );

  const handleCancelPress = useCallback(() => {
    setIsConfirmMoveModalOpen(false);
  }, []);

  const handleSavePress = useCallback(() => {
    if (isPathChanging && !isConfirmMoveModalOpen) {
      setIsConfirmMoveModalOpen(true);
    } else {
      setIsConfirmMoveModalOpen(false);

      dispatch(
        saveSeries({
          id: seriesId,
          moveFiles: false,
        })
      );
    }
  }, [seriesId, isPathChanging, isConfirmMoveModalOpen, dispatch]);

  const handleMoveSeriesPress = useCallback(() => {
    setIsConfirmMoveModalOpen(false);

    dispatch(
      saveSeries({
        id: seriesId,
        moveFiles: true,
      })
    );
  }, [seriesId, dispatch]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('EditSeriesModalHeader', { title })}</ModalHeader>

      <ModalBody>
        <Form {...otherSettings}>
          <FormGroup>
            <FormLabel>{translate('Monitored')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="monitored"
              helpText={translate('MonitoredEpisodesHelpText')}
              {...settings.monitored}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>
              {translate('MonitorNewSeasons')}
              <Popover
                anchor={<Icon className={styles.labelIcon} name={icons.INFO} />}
                title={translate('MonitorNewSeasons')}
                body={<SeriesMonitorNewItemsOptionsPopoverContent />}
                position={tooltipPositions.RIGHT}
              />
            </FormLabel>

            <FormInputGroup
              type={inputTypes.MONITOR_NEW_ITEMS_SELECT}
              name="monitorNewItems"
              helpText={translate('MonitorNewSeasonsHelpText')}
              {...settings.monitorNewItems}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('UseSeasonFolder')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="seasonFolder"
              helpText={translate('UseSeasonFolderHelpText')}
              {...settings.seasonFolder}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('QualityProfile')}</FormLabel>

            <FormInputGroup
              type={inputTypes.QUALITY_PROFILE_SELECT}
              name="qualityProfileId"
              {...settings.qualityProfileId}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('SeriesType')}</FormLabel>

            <FormInputGroup
              type={inputTypes.SERIES_TYPE_SELECT}
              name="seriesType"
              {...settings.seriesType}
              helpText={translate('SeriesTypesHelpText')}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('Path')}</FormLabel>

            <FormInputGroup
              type={inputTypes.PATH}
              name="path"
              {...settings.path}
              buttons={[
                <FormInputButton
                  key="fileBrowser"
                  kind={kinds.DEFAULT}
                  title="Root Folder"
                  onPress={handleRootFolderPress}
                >
                  <Icon name={icons.ROOT_FOLDER} />
                </FormInputButton>,
              ]}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('Tags')}</FormLabel>

            <FormInputGroup
              type={inputTypes.TAG}
              name="tags"
              {...settings.tags}
              onChange={handleInputChange}
            />
          </FormGroup>
        </Form>
      </ModalBody>

      <ModalFooter>
        <Button
          className={styles.deleteButton}
          kind={kinds.DANGER}
          onPress={onDeleteSeriesPress}
        >
          {translate('Delete')}
        </Button>

        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <SpinnerErrorButton
          error={saveError}
          isSpinning={isSaving}
          onPress={handleSavePress}
        >
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>

      <RootFolderModal
        isOpen={isRootFolderModalOpen}
        seriesId={seriesId}
        rootFolderPath={rootFolderPath}
        onSavePress={handleRootFolderChange}
        onModalClose={handleRootFolderModalClose}
      />

      <MoveSeriesModal
        originalPath={path}
        destinationPath={pendingChanges.path}
        isOpen={isConfirmMoveModalOpen}
        onModalClose={handleCancelPress}
        onSavePress={handleSavePress}
        onMoveSeriesPress={handleMoveSeriesPress}
      />
    </ModalContent>
  );
}

export default EditSeriesModalContent;
