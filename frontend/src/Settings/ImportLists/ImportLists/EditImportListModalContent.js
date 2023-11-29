import PropTypes from 'prop-types';
import React from 'react';
import SeriesMonitoringOptionsPopoverContent from 'AddSeries/SeriesMonitoringOptionsPopoverContent';
import SeriesMonitorNewItemsOptionsPopoverContent from 'AddSeries/SeriesMonitorNewItemsOptionsPopoverContent';
import SeriesTypePopoverContent from 'AddSeries/SeriesTypePopoverContent';
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
import { icons, inputTypes, kinds, tooltipPositions } from 'Helpers/Props';
import formatShortTimeSpan from 'Utilities/Date/formatShortTimeSpan';
import translate from 'Utilities/String/translate';
import styles from './EditImportListModalContent.css';

function EditImportListModalContent(props) {

  const {
    advancedSettings,
    isFetching,
    error,
    isSaving,
    isTesting,
    saveError,
    item,
    onInputChange,
    onFieldChange,
    onModalClose,
    onSavePress,
    onTestPress,
    onDeleteImportListPress,
    ...otherProps
  } = props;

  const {
    id,
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
    fields
  } = item;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id ? translate('EditImportListImplementation', { implementationName }) : translate('AddImportListImplementation', { implementationName })}
      </ModalHeader>

      <ModalBody>
        {
          isFetching ?
            <LoadingIndicator /> :
            null
        }

        {
          !isFetching && !!error ?
            <div>
              {translate('AddListError')}
            </div> :
            null
        }

        {
          !isFetching && !error ?
            <Form {...otherProps}>

              <Alert
                kind={kinds.INFO}
                className={styles.message}
              >
                {translate('ListWillRefreshEveryInterval', {
                  refreshInterval: formatShortTimeSpan(minRefreshInterval.value)
                })}
              </Alert>

              <FormGroup>
                <FormLabel>{translate('Name')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.TEXT}
                  name="name"
                  {...name}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('EnableAutomaticAdd')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="enableAutomaticAdd"
                  helpText={translate('EnableAutomaticAddSeriesHelpText')}
                  {...enableAutomaticAdd}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('ImportListSearchForMissingEpisodes')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="searchForMissingEpisodes"
                  helpText={translate('ImportListSearchForMissingEpisodesHelpText')}
                  {...searchForMissingEpisodes}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>
                  {translate('Monitor')}

                  <Popover
                    anchor={
                      <Icon
                        className={styles.labelIcon}
                        name={icons.INFO}
                      />
                    }
                    title={translate('MonitoringOptions')}
                    body={<SeriesMonitoringOptionsPopoverContent />}
                    position={tooltipPositions.RIGHT}
                  />
                </FormLabel>

                <FormInputGroup
                  type={inputTypes.MONITOR_EPISODES_SELECT}
                  name="shouldMonitor"
                  onChange={onInputChange}
                  {...shouldMonitor}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>
                  {translate('MonitorNewSeasons')}
                  <Popover
                    anchor={
                      <Icon
                        className={styles.labelIcon}
                        name={icons.INFO}
                      />
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
                  onChange={onInputChange}
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
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('QualityProfile')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.QUALITY_PROFILE_SELECT}
                  name="qualityProfileId"
                  helpText={translate('ListQualityProfileHelpText')}
                  {...qualityProfileId}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>
                  {translate('SeriesType')}

                  <Popover
                    anchor={
                      <Icon
                        className={styles.labelIcon}
                        name={icons.INFO}
                      />
                    }
                    title={translate('SeriesTypes')}
                    body={<SeriesTypePopoverContent />}
                    position={tooltipPositions.RIGHT}
                  />
                </FormLabel>

                <FormInputGroup
                  type={inputTypes.SERIES_TYPE_SELECT}
                  name="seriesType"
                  onChange={onInputChange}
                  {...seriesType}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('SeasonFolder')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="seasonFolder"
                  onChange={onInputChange}
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
                  onChange={onInputChange}
                />
              </FormGroup>

              {
                !!fields && !!fields.length &&
                  <div>
                    {
                      fields.map((field) => {
                        return (
                          <ProviderFieldFormGroup
                            key={field.name}
                            advancedSettings={advancedSettings}
                            provider="importList"
                            providerData={item}
                            section="settings.importLists"
                            {...field}
                            onChange={onFieldChange}
                          />
                        );
                      })
                    }
                  </div>
              }

            </Form> :
            null
        }
      </ModalBody>
      <ModalFooter>
        {
          id &&
            <Button
              className={styles.deleteButton}
              kind={kinds.DANGER}
              onPress={onDeleteImportListPress}
            >
              {translate('Delete')}
            </Button>
        }

        <SpinnerErrorButton
          isSpinning={isTesting}
          error={saveError}
          onPress={onTestPress}
        >
          {translate('Test')}
        </SpinnerErrorButton>

        <Button
          onPress={onModalClose}
        >
          {translate('Cancel')}
        </Button>

        <SpinnerErrorButton
          isSpinning={isSaving}
          error={saveError}
          onPress={onSavePress}
        >
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

EditImportListModalContent.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isSaving: PropTypes.bool.isRequired,
  isTesting: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onFieldChange: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onTestPress: PropTypes.func.isRequired,
  onDeleteImportListPress: PropTypes.func
};

export default EditImportListModalContent;
