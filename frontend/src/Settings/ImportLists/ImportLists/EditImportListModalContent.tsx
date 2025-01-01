import React, { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import SeriesMonitoringOptionsPopoverContent from 'AddSeries/SeriesMonitoringOptionsPopoverContent';
import SeriesMonitorNewItemsOptionsPopoverContent from 'AddSeries/SeriesMonitorNewItemsOptionsPopoverContent';
import SeriesTypePopoverContent from 'AddSeries/SeriesTypePopoverContent';
import { ImportListAppState } from 'App/State/SettingsAppState';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import ProviderFieldFormGroup from 'Components/Form/ProviderFieldFormGroup';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Popover from 'Components/Tooltip/Popover';
import usePrevious from 'Helpers/Hooks/usePrevious';
import useShowAdvancedSettings from 'Helpers/Hooks/useShowAdvancedSettings';
import { icons, inputTypes, kinds, tooltipPositions } from 'Helpers/Props';
import AdvancedSettingsButton from 'Settings/AdvancedSettingsButton';
import {
  saveImportList,
  setImportListFieldValue,
  setImportListValue,
  testImportList,
} from 'Store/Actions/settingsActions';
import { createProviderSettingsSelectorHook } from 'Store/Selectors/createProviderSettingsSelector';
import ImportList from 'typings/ImportList';
import { InputChanged } from 'typings/inputs';
import formatShortTimeSpan from 'Utilities/Date/formatShortTimeSpan';
import translate from 'Utilities/String/translate';
import styles from './EditImportListModalContent.css';

export interface EditImportListModalContentProps {
  id?: number;
  onModalClose: () => void;
  onDeleteImportListPress?: () => void;
}

function EditImportListModalContent({
  id,
  onModalClose,
  onDeleteImportListPress,
}: EditImportListModalContentProps) {
  const dispatch = useDispatch();
  const showAdvancedSettings = useShowAdvancedSettings();

  const {
    isFetching,
    isSaving,
    isTesting = false,
    error,
    saveError,
    item,
    validationErrors,
    validationWarnings,
  } = useSelector(
    createProviderSettingsSelectorHook<ImportList, ImportListAppState>(
      'importLists',
      id
    )
  );

  const wasSaving = usePrevious(isSaving);

  const {
    implementationName,
    name,
    enableAutomaticAdd,
    searchForMissingEpisodes,
    minRefreshInterval,
    shouldMonitor,
    rootFolderPath,
    monitorNewItems,
    qualityProfileId,
    seriesType,
    seasonFolder,
    tags,
    fields,
  } = item;

  const handleInputChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error - actions are not typed
      dispatch(setImportListValue(change));
    },
    [dispatch]
  );

  const handleFieldChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error - actions are not typed
      dispatch(setImportListFieldValue(change));
    },
    [dispatch]
  );

  const handleTestPress = useCallback(() => {
    dispatch(testImportList({ id }));
  }, [id, dispatch]);

  const handleSavePress = useCallback(() => {
    dispatch(saveImportList({ id }));
  }, [id, dispatch]);

  useEffect(() => {
    if (wasSaving && !isSaving && !saveError) {
      onModalClose();
    }
  }, [isSaving, wasSaving, saveError, onModalClose]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id
          ? translate('EditImportListImplementation', { implementationName })
          : translate('AddImportListImplementation', { implementationName })}
      </ModalHeader>

      <ModalBody>
        {isFetching ? <LoadingIndicator /> : null}

        {!isFetching && !!error ? (
          <Alert kind={kinds.DANGER}>{translate('AddListError')}</Alert>
        ) : null}

        {!isFetching && !error ? (
          <Form
            validationErrors={validationErrors}
            validationWarnings={validationWarnings}
          >
            <Alert kind={kinds.INFO} className={styles.message}>
              {translate('ListWillRefreshEveryInterval', {
                refreshInterval: formatShortTimeSpan(minRefreshInterval.value),
              })}
            </Alert>

            <FormGroup>
              <FormLabel>{translate('Name')}</FormLabel>

              <FormInputGroup
                type={inputTypes.TEXT}
                name="name"
                {...name}
                onChange={handleInputChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('EnableAutomaticAdd')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="enableAutomaticAdd"
                helpText={translate('EnableAutomaticAddSeriesHelpText')}
                {...enableAutomaticAdd}
                onChange={handleInputChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>
                {translate('ImportListSearchForMissingEpisodes')}
              </FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="searchForMissingEpisodes"
                helpText={translate(
                  'ImportListSearchForMissingEpisodesHelpText'
                )}
                {...searchForMissingEpisodes}
                onChange={handleInputChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>
                {translate('Monitor')}

                <Popover
                  anchor={
                    <Icon className={styles.labelIcon} name={icons.INFO} />
                  }
                  title={translate('MonitoringOptions')}
                  body={<SeriesMonitoringOptionsPopoverContent />}
                  position={tooltipPositions.RIGHT}
                />
              </FormLabel>

              <FormInputGroup
                type={inputTypes.MONITOR_EPISODES_SELECT}
                name="shouldMonitor"
                onChange={handleInputChange}
                {...shouldMonitor}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>
                {translate('MonitorNewSeasons')}
                <Popover
                  anchor={
                    <Icon className={styles.labelIcon} name={icons.INFO} />
                  }
                  title={translate('MonitorNewSeasons')}
                  body={<SeriesMonitorNewItemsOptionsPopoverContent />}
                  position={tooltipPositions.RIGHT}
                />
              </FormLabel>

              <FormInputGroup
                type={inputTypes.MONITOR_NEW_ITEMS_SELECT}
                name="monitorNewItems"
                helpText={translate('MonitorNewSeasonsHelpText')}
                {...monitorNewItems}
                onChange={handleInputChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('RootFolder')}</FormLabel>

              <FormInputGroup
                type={inputTypes.ROOT_FOLDER_SELECT}
                name="rootFolderPath"
                helpText={translate('ListRootFolderHelpText')}
                {...rootFolderPath}
                includeMissingValue={true}
                onChange={handleInputChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('QualityProfile')}</FormLabel>

              <FormInputGroup
                type={inputTypes.QUALITY_PROFILE_SELECT}
                name="qualityProfileId"
                helpText={translate('ListQualityProfileHelpText')}
                {...qualityProfileId}
                onChange={handleInputChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>
                {translate('SeriesType')}

                <Popover
                  anchor={
                    <Icon className={styles.labelIcon} name={icons.INFO} />
                  }
                  title={translate('SeriesTypes')}
                  body={<SeriesTypePopoverContent />}
                  position={tooltipPositions.RIGHT}
                />
              </FormLabel>

              <FormInputGroup
                type={inputTypes.SERIES_TYPE_SELECT}
                name="seriesType"
                onChange={handleInputChange}
                {...seriesType}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('SeasonFolder')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="seasonFolder"
                onChange={handleInputChange}
                {...seasonFolder}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('SonarrTags')}</FormLabel>

              <FormInputGroup
                type={inputTypes.TAG}
                name="tags"
                helpText={translate('ListTagsHelpText')}
                {...tags}
                onChange={handleInputChange}
              />
            </FormGroup>

            {fields?.length ? (
              <div>
                {fields.map((field) => {
                  return (
                    <ProviderFieldFormGroup
                      key={field.name}
                      {...field}
                      advancedSettings={showAdvancedSettings}
                      provider="importList"
                      providerData={item}
                      onChange={handleFieldChange}
                    />
                  );
                })}
              </div>
            ) : null}
          </Form>
        ) : null}
      </ModalBody>
      <ModalFooter>
        {id && (
          <Button
            className={styles.deleteButton}
            kind={kinds.DANGER}
            onPress={onDeleteImportListPress}
          >
            {translate('Delete')}
          </Button>
        )}

        <AdvancedSettingsButton showLabel={false} />

        <SpinnerErrorButton
          isSpinning={isTesting}
          error={saveError}
          onPress={handleTestPress}
        >
          {translate('Test')}
        </SpinnerErrorButton>

        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <SpinnerErrorButton
          isSpinning={isSaving}
          error={saveError}
          onPress={handleSavePress}
        >
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

export default EditImportListModalContent;
