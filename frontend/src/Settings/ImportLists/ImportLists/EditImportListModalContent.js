import PropTypes from 'prop-types';
import React from 'react';
import SeriesTypePopoverContent from 'AddSeries/SeriesTypePopoverContent';
import SeriesMonitoringOptionsPopoverContent from 'AddSeries/SeriesMonitoringOptionsPopoverContent';
import { icons, inputTypes, kinds, tooltipPositions } from 'Helpers/Props';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputGroup from 'Components/Form/FormInputGroup';
import Popover from 'Components/Tooltip/Popover';
import ProviderFieldFormGroup from 'Components/Form/ProviderFieldFormGroup';
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
    showLanguageProfile,
    ...otherProps
  } = props;

  const {
    id,
    name,
    enableAutomaticAdd,
    shouldMonitor,
    rootFolderPath,
    qualityProfileId,
    languageProfileId,
    seriesType,
    seasonFolder,
    tags,
    fields
  } = item;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id ? 'Edit List' : 'Add List'}
      </ModalHeader>

      <ModalBody>
        {
          isFetching ?
            <LoadingIndicator /> :
            null
        }

        {
          !isFetching && !!error ?
            <div>Unable to add a new list, please try again.</div> :
            null
        }

        {
          !isFetching && !error ?
            <Form {...otherProps}>
              <FormGroup>
                <FormLabel>Name</FormLabel>

                <FormInputGroup
                  type={inputTypes.TEXT}
                  name="name"
                  {...name}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Enable Automatic Add</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="enableAutomaticAdd"
                  helpText={'Add series to Sonarr when syncs are performed via the UI or by Sonarr'}
                  {...enableAutomaticAdd}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>
                  Monitor

                  <Popover
                    anchor={
                      <Icon
                        className={styles.labelIcon}
                        name={icons.INFO}
                      />
                    }
                    title="Monitoring Options"
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
                <FormLabel>Root Folder</FormLabel>

                <FormInputGroup
                  type={inputTypes.ROOT_FOLDER_SELECT}
                  name="rootFolderPath"
                  helpText={'Root Folder list items will be added to'}
                  {...rootFolderPath}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Quality Profile</FormLabel>

                <FormInputGroup
                  type={inputTypes.QUALITY_PROFILE_SELECT}
                  name="qualityProfileId"
                  helpText={'Quality Profile list items will be added with'}
                  {...qualityProfileId}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup className={showLanguageProfile ? undefined : styles.hideLanguageProfile}>
                <FormLabel>Language Profile</FormLabel>

                <FormInputGroup
                  type={inputTypes.LANGUAGE_PROFILE_SELECT}
                  name="languageProfileId"
                  helpText={'Language Profile list items will be added with'}
                  {...languageProfileId}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>
                  Series Type

                  <Popover
                    anchor={
                      <Icon
                        className={styles.labelIcon}
                        name={icons.INFO}
                      />
                    }
                    title="Series Types"
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
                <FormLabel>Season Folder</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="seasonFolder"
                  onChange={onInputChange}
                  {...seasonFolder}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Sonarr Tags</FormLabel>

                <FormInputGroup
                  type={inputTypes.TAG}
                  name="tags"
                  helpText="Tags list items will be added with"
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
              Delete
            </Button>
        }

        <SpinnerErrorButton
          isSpinning={isTesting}
          error={saveError}
          onPress={onTestPress}
        >
          Test
        </SpinnerErrorButton>

        <Button
          onPress={onModalClose}
        >
          Cancel
        </Button>

        <SpinnerErrorButton
          isSpinning={isSaving}
          error={saveError}
          onPress={onSavePress}
        >
          Save
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
  showLanguageProfile: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onFieldChange: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onTestPress: PropTypes.func.isRequired,
  onDeleteImportListPress: PropTypes.func
};

export default EditImportListModalContent;
