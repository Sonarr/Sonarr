import React, { useCallback, useEffect } from 'react';
import SeriesMonitoringOptionsPopoverContent from 'AddSeries/SeriesMonitoringOptionsPopoverContent';
import SeriesMonitorNewItemsOptionsPopoverContent from 'AddSeries/SeriesMonitorNewItemsOptionsPopoverContent';
import SeriesTypePopoverContent from 'AddSeries/SeriesTypePopoverContent';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import ProviderFieldFormGroup from 'Components/Form/ProviderFieldFormGroup';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalSection from 'Components/ModalSection';
import Popover from 'Components/Tooltip/Popover';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { icons, inputTypes, kinds, tooltipPositions } from 'Helpers/Props';
import AdvancedSettingsButton from 'Settings/AdvancedSettingsButton';
import { useShowAdvancedSettings } from 'Settings/advancedSettingsStore';
import { SelectedSchema } from 'Settings/useProviderSchema';
import { EnhancedSelectInputChanged, InputChanged } from 'typings/inputs';
import formatShortTimeSpan from 'Utilities/Date/formatShortTimeSpan';
import translate from 'Utilities/String/translate';
import { useManageImportList } from './useImportLists';
import styles from './EditImportListModalContent.css';

export interface EditImportListModalContentProps {
  id?: number;
  cloneId?: number;
  selectedSchema?: SelectedSchema;
  onModalClose: () => void;
  onDeleteImportListPress?: () => void;
}

function EditImportListModalContent({
  id,
  cloneId,
  selectedSchema,
  onModalClose,
  onDeleteImportListPress,
}: EditImportListModalContentProps) {
  const showAdvancedSettings = useShowAdvancedSettings();

  const {
    item,
    updateFieldValue,
    updateValue,
    saveProvider,
    isSaving,
    saveError,
    testProvider,
    isTesting,
    validationErrors,
    validationWarnings,
  } = useManageImportList(id, cloneId, selectedSchema);

  const wasSaving = usePrevious(isSaving);

  const {
    implementationName = '',
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
    tagExisting,
    fields,
  } = item;

  const handleInputChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error - InputChanged is not typed correctly
      updateValue(change.name, change.value);
    },
    [updateValue]
  );

  const handleFieldChange = useCallback(
    ({
      name,
      value,
      additionalProperties,
    }: EnhancedSelectInputChanged<unknown>) => {
      updateFieldValue({ [name]: value, ...additionalProperties });
    },
    [updateFieldValue]
  );

  const handleTestPress = useCallback(() => {
    testProvider();
  }, [testProvider]);

  const handleSavePress = useCallback(() => {
    saveProvider();
  }, [saveProvider]);

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
        <Form
          validationErrors={validationErrors}
          validationWarnings={validationWarnings}
        >
          <Alert kind={kinds.INFO} className={styles.message}>
            {translate('ListWillRefreshEveryInterval', {
              refreshInterval: formatShortTimeSpan(minRefreshInterval.value),
            })}
          </Alert>

          <FormRow>
            <FormLabel>{translate('Name')}</FormLabel>

            <FormInput
              type={inputTypes.TEXT}
              name="name"
              {...name}
              onChange={handleInputChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('EnableAutomaticAdd')}</FormLabel>
            <FormInputHelpText
              text={translate('EnableAutomaticAddSeriesHelpText')}
            />
            <FormInput
              type={inputTypes.CHECK}
              name="enableAutomaticAdd"
              {...enableAutomaticAdd}
              onChange={handleInputChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>
              {translate('ImportListSearchForMissingEpisodes')}
            </FormLabel>
            <FormInputHelpText
              text={translate('ImportListSearchForMissingEpisodesHelpText')}
            />
            <FormInput
              type={inputTypes.CHECK}
              name="searchForMissingEpisodes"
              {...searchForMissingEpisodes}
              onChange={handleInputChange}
            />
          </FormRow>

          <ModalSection title={translate('Series')}>
            <FormRow>
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

              <FormInput
                type={inputTypes.MONITOR_EPISODES_SELECT}
                name="shouldMonitor"
                onChange={handleInputChange}
                {...shouldMonitor}
              />
            </FormRow>

            <FormRow>
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
              <FormInputHelpText
                text={translate('MonitorNewSeasonsHelpText')}
              />
              <FormInput
                type={inputTypes.MONITOR_NEW_ITEMS_SELECT}
                name="monitorNewItems"
                {...monitorNewItems}
                onChange={handleInputChange}
              />
            </FormRow>

            <FormRow>
              <FormLabel>{translate('RootFolder')}</FormLabel>
              <FormInputHelpText text={translate('ListRootFolderHelpText')} />
              <FormInput
                type={inputTypes.ROOT_FOLDER_SELECT}
                name="rootFolderPath"
                {...rootFolderPath}
                includeMissingValue={true}
                onChange={handleInputChange}
              />
            </FormRow>

            <FormRow>
              <FormLabel>{translate('QualityProfile')}</FormLabel>
              <FormInputHelpText
                text={translate('ListQualityProfileHelpText')}
              />
              <FormInput
                type={inputTypes.QUALITY_PROFILE_SELECT}
                name="qualityProfileId"
                {...qualityProfileId}
                onChange={handleInputChange}
              />
            </FormRow>

            <FormRow>
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

              <FormInput
                type={inputTypes.SERIES_TYPE_SELECT}
                name="seriesType"
                onChange={handleInputChange}
                {...seriesType}
              />
            </FormRow>

            <FormRow>
              <FormLabel>{translate('SeasonFolder')}</FormLabel>

              <FormInput
                type={inputTypes.CHECK}
                name="seasonFolder"
                onChange={handleInputChange}
                {...seasonFolder}
              />
            </FormRow>
          </ModalSection>

          {fields?.map((field) => {
            return (
              <ProviderFieldFormGroup
                key={field.name}
                advancedSettings={showAdvancedSettings}
                provider="importList"
                providerData={item}
                layout="row"
                {...field}
                onChange={handleFieldChange}
              />
            );
          })}

          <FormRow>
            <FormLabel>{translate('SonarrTags')}</FormLabel>
            <FormInputHelpText text={translate('ListTagsHelpText')} />
            <FormInput
              type={inputTypes.TAG}
              name="tags"
              {...tags}
              onChange={handleInputChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('TagExisting')}</FormLabel>
            <FormInputHelpText text={translate('TagExistingHelpText')} />
            <FormInput
              type={inputTypes.CHECK}
              name="tagExisting"
              {...tagExisting}
              onChange={handleInputChange}
            />
          </FormRow>
        </Form>
      </ModalBody>

      <ModalFooter>
        {id ? (
          <Button
            className={styles.deleteButton}
            kind={kinds.DANGER}
            onPress={onDeleteImportListPress}
          >
            {translate('Delete')}
          </Button>
        ) : null}

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
