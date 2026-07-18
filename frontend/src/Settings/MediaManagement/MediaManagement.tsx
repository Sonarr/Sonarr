import React, { useCallback, useRef, useState } from 'react';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import { EnhancedSelectInputValue } from 'Components/Form/Select/EnhancedSelectInput';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContentBody from 'Components/Page/PageContentBody';
import PageHeading from 'Components/Page/PageHeading';
import { inputTypes, kinds, sizes } from 'Helpers/Props';
import RootFolders from 'RootFolder/RootFolders';
import { useShowAdvancedSettings } from 'Settings/advancedSettingsStore';
import settingsStyles from 'Settings/Settings.css';
import SettingsPage from 'Settings/SettingsPage';
import { useIsWindows } from 'System/Status/useSystemStatus';
import { InputChanged } from 'typings/inputs';
import { SettingsStateChange } from 'typings/Settings/SettingsState';
import translate from 'Utilities/String/translate';
import Naming from './Naming/Naming';
import AddRootFolder from './RootFolder/AddRootFolder';
import {
  MediaManagementSettingsModel,
  useManageMediaManagementSettings,
} from './useMediaManagementSettings';

const episodeTitleRequiredOptions: EnhancedSelectInputValue<string>[] = [
  {
    key: 'always',
    get value() {
      return translate('Always');
    },
  },
  {
    key: 'bulkSeasonReleases',
    get value() {
      return translate('OnlyForBulkSeasonReleases');
    },
  },
  {
    key: 'never',
    get value() {
      return translate('Never');
    },
  },
];

const rescanAfterRefreshOptions: EnhancedSelectInputValue<string>[] = [
  {
    key: 'always',
    get value() {
      return translate('Always');
    },
  },
  {
    key: 'afterManual',
    get value() {
      return translate('AfterManualRefresh');
    },
  },
  {
    key: 'never',
    get value() {
      return translate('Never');
    },
  },
];

const downloadPropersAndRepacksOptions: EnhancedSelectInputValue<string>[] = [
  {
    key: 'preferAndUpgrade',
    get value() {
      return translate('PreferAndUpgrade');
    },
  },
  {
    key: 'doNotUpgrade',
    get value() {
      return translate('DoNotUpgradeAutomatically');
    },
  },
  {
    key: 'doNotPrefer',
    get value() {
      return translate('DoNotPrefer');
    },
  },
];

const fileDateOptions: EnhancedSelectInputValue<string>[] = [
  {
    key: 'none',
    get value() {
      return translate('None');
    },
  },
  {
    key: 'localAirDate',
    get value() {
      return translate('LocalAirDate');
    },
  },
  {
    key: 'utcAirDate',
    get value() {
      return translate('UtcAirDate');
    },
  },
];

const seasonPackUpgradeOptions: EnhancedSelectInputValue<string>[] = [
  {
    key: 'all',
    get value() {
      return translate('All');
    },
  },
  {
    key: 'threshold',
    get value() {
      return translate('Threshold');
    },
  },
  {
    key: 'any',
    get value() {
      return translate('Any');
    },
  },
];

function MediaManagement() {
  const showAdvancedSettings = useShowAdvancedSettings();
  const isWindows = useIsWindows();

  const {
    isFetching,
    isFetched: isPopulated,
    isSaving,
    error,
    settings,
    hasSettings,
    hasPendingChanges,
    validationErrors,
    validationWarnings,
    saveSettings: saveMediaManagementSettings,
    updateSetting,
  } = useManageMediaManagementSettings();

  const [naming, setNaming] = useState<SettingsStateChange>({
    isSaving: false,
    hasPendingChanges: false,
  });

  const saveSettings = useRef<{
    naming: () => void;
  }>({
    naming: () => {},
  });

  const handleSetNamingSave = useCallback((saveCallback: () => void) => {
    saveSettings.current.naming = saveCallback;
  }, []);

  const handleSavePress = useCallback(() => {
    saveMediaManagementSettings();
    saveSettings.current.naming();
  }, [saveMediaManagementSettings]);

  const handleInputChange = useCallback(
    (change: InputChanged) => {
      updateSetting(
        change.name as keyof MediaManagementSettingsModel,
        change.value as MediaManagementSettingsModel[keyof MediaManagementSettingsModel]
      );
    },
    [updateSetting]
  );

  return (
    <SettingsPage
      title={translate('MediaManagementSettings')}
      isSaving={isSaving || naming.isSaving}
      hasPendingChanges={naming.hasPendingChanges || hasPendingChanges}
      onSavePress={handleSavePress}
    >
      <PageContentBody>
        <div className={settingsStyles.section}>
          <PageHeading
            scope={translate('Settings')}
            title={translate('MediaManagement')}
          />

          <Naming
            setChildSave={handleSetNamingSave}
            onChildStateChange={setNaming}
          />

          {isFetching ? (
            <FieldSet legend={translate('NamingSettings')}>
              <LoadingIndicator />
            </FieldSet>
          ) : null}

          {!isFetching && error ? (
            <FieldSet legend={translate('NamingSettings')}>
              <Alert kind={kinds.DANGER}>
                {translate('MediaManagementSettingsLoadError')}
              </Alert>
            </FieldSet>
          ) : null}

          {hasSettings && isPopulated && !error ? (
            <Form
              id="mediaManagementSettings"
              validationErrors={validationErrors}
              validationWarnings={validationWarnings}
            >
              {showAdvancedSettings ? (
                <FieldSet
                  legend={translate('Folders')}
                  caption={translate('FoldersCaption')}
                >
                  <FormRow
                    advancedSettings={showAdvancedSettings}
                    isAdvanced={true}
                    size={sizes.MEDIUM}
                  >
                    <FormLabel>
                      {translate('CreateEmptySeriesFolders')}
                    </FormLabel>
                    <FormInputHelpText
                      text={translate('CreateEmptySeriesFoldersHelpText')}
                    />
                    <FormInput
                      type={inputTypes.CHECK}
                      name="createEmptySeriesFolders"
                      onChange={handleInputChange}
                      {...settings.createEmptySeriesFolders}
                    />
                  </FormRow>

                  <FormRow
                    advancedSettings={showAdvancedSettings}
                    isAdvanced={true}
                    size={sizes.MEDIUM}
                  >
                    <FormLabel>{translate('DeleteEmptyFolders')}</FormLabel>
                    <FormInputHelpText
                      text={translate('DeleteEmptySeriesFoldersHelpText')}
                    />
                    <FormInput
                      type={inputTypes.CHECK}
                      name="deleteEmptyFolders"
                      onChange={handleInputChange}
                      {...settings.deleteEmptyFolders}
                    />
                  </FormRow>
                </FieldSet>
              ) : null}

              {showAdvancedSettings ? (
                <FieldSet
                  legend={translate('Importing')}
                  caption={translate('ImportingCaption')}
                >
                  <FormRow
                    advancedSettings={showAdvancedSettings}
                    isAdvanced={true}
                    size={sizes.SMALL}
                  >
                    <FormLabel>{translate('EpisodeTitleRequired')}</FormLabel>
                    <FormInputHelpText
                      text={translate('EpisodeTitleRequiredHelpText')}
                    />
                    <FormInput
                      type={inputTypes.SELECT}
                      name="episodeTitleRequired"
                      values={episodeTitleRequiredOptions}
                      onChange={handleInputChange}
                      {...settings.episodeTitleRequired}
                    />
                  </FormRow>

                  <FormRow
                    advancedSettings={showAdvancedSettings}
                    isAdvanced={true}
                    size={sizes.MEDIUM}
                  >
                    <FormLabel>
                      {translate('SkipFreeSpaceCheckWhenGrabbing')}
                    </FormLabel>
                    <FormInputHelpText
                      text={translate('SkipFreeSpaceCheckWhenGrabbingHelpText')}
                    />
                    <FormInput
                      type={inputTypes.CHECK}
                      name="skipFreeSpaceCheckWhenGrabbing"
                      onChange={handleInputChange}
                      {...settings.skipFreeSpaceCheckWhenGrabbing}
                    />
                  </FormRow>

                  <FormRow
                    advancedSettings={showAdvancedSettings}
                    isAdvanced={true}
                    size={sizes.MEDIUM}
                  >
                    <FormLabel>
                      {translate('SkipFreeSpaceCheckWhenImporting')}
                    </FormLabel>
                    <FormInputHelpText
                      text={translate(
                        'SkipFreeSpaceCheckWhenImportingHelpText'
                      )}
                    />
                    <FormInput
                      type={inputTypes.CHECK}
                      name="skipFreeSpaceCheckWhenImporting"
                      onChange={handleInputChange}
                      {...settings.skipFreeSpaceCheckWhenImporting}
                    />
                  </FormRow>

                  <FormRow
                    advancedSettings={showAdvancedSettings}
                    isAdvanced={true}
                    size={sizes.MEDIUM}
                  >
                    <FormLabel>{translate('MinimumFreeSpace')}</FormLabel>
                    <FormInputHelpText
                      text={translate('MinimumFreeSpaceHelpText')}
                    />
                    <FormInput
                      type={inputTypes.NUMBER}
                      unit="MB"
                      name="minimumFreeSpaceWhenImporting"
                      onChange={handleInputChange}
                      {...settings.minimumFreeSpaceWhenImporting}
                    />
                  </FormRow>

                  <FormRow
                    advancedSettings={showAdvancedSettings}
                    isAdvanced={true}
                    size={sizes.MEDIUM}
                  >
                    <FormLabel>
                      {translate('UseHardlinksInsteadOfCopy')}
                    </FormLabel>
                    <FormInputHelpText
                      text={translate('CopyUsingHardlinksSeriesHelpText')}
                    />
                    <FormInputHelpText
                      text={translate('CopyUsingHardlinksHelpTextWarning')}
                      isWarning={true}
                    />
                    <FormInput
                      type={inputTypes.CHECK}
                      name="copyUsingHardlinks"
                      onChange={handleInputChange}
                      {...settings.copyUsingHardlinks}
                    />
                  </FormRow>

                  <FormRow
                    advancedSettings={showAdvancedSettings}
                    isAdvanced={true}
                    size={sizes.MEDIUM}
                  >
                    <FormLabel>{translate('ImportUsingScript')}</FormLabel>
                    <FormInputHelpText
                      text={translate('ImportUsingScriptHelpText')}
                    />
                    <FormInput
                      type={inputTypes.CHECK}
                      name="useScriptImport"
                      onChange={handleInputChange}
                      {...settings.useScriptImport}
                    />
                  </FormRow>

                  {settings.useScriptImport.value ? (
                    <FormRow
                      advancedSettings={showAdvancedSettings}
                      isAdvanced={true}
                    >
                      <FormLabel>{translate('ImportScriptPath')}</FormLabel>
                      <FormInputHelpText
                        text={translate('ImportScriptPathHelpText')}
                      />
                      <FormInput
                        type={inputTypes.PATH}
                        includeFiles={true}
                        name="scriptImportPath"
                        onChange={handleInputChange}
                        {...settings.scriptImportPath}
                      />
                    </FormRow>
                  ) : null}

                  <FormRow size={sizes.MEDIUM}>
                    <FormLabel>{translate('ImportExtraFiles')}</FormLabel>
                    <FormInputHelpText
                      text={translate('ImportExtraFilesEpisodeHelpText')}
                    />
                    <FormInput
                      type={inputTypes.CHECK}
                      name="importExtraFiles"
                      onChange={handleInputChange}
                      {...settings.importExtraFiles}
                    />
                  </FormRow>

                  {settings.importExtraFiles.value ? (
                    <FormRow
                      advancedSettings={showAdvancedSettings}
                      isAdvanced={true}
                    >
                      <FormLabel>{translate('ImportExtraFiles')}</FormLabel>
                      <FormInputHelpText
                        text={translate('ExtraFileExtensionsHelpText')}
                      />
                      <FormInputHelpText
                        text={translate('ExtraFileExtensionsHelpTextsExamples')}
                      />
                      <FormInput
                        type={inputTypes.TEXT}
                        name="extraFileExtensions"
                        onChange={handleInputChange}
                        {...settings.extraFileExtensions}
                      />
                    </FormRow>
                  ) : null}

                  <FormRow
                    advancedSettings={showAdvancedSettings}
                    isAdvanced={true}
                  >
                    <FormLabel>{translate('UserRejectedExtensions')}</FormLabel>
                    <FormInputHelpText
                      text={translate('UserRejectedExtensionsHelpText')}
                    />
                    <FormInputHelpText
                      text={translate('UserRejectedExtensionsTextsExamples')}
                    />
                    <FormInput
                      type={inputTypes.TEXT}
                      name="userRejectedExtensions"
                      onChange={handleInputChange}
                      {...settings.userRejectedExtensions}
                    />
                  </FormRow>

                  {showAdvancedSettings ? (
                    <>
                      <FormRow
                        advancedSettings={showAdvancedSettings}
                        isAdvanced={true}
                        size={sizes.MEDIUM}
                      >
                        <FormLabel>
                          {translate('SeasonPackUpgradeAllowLabel')}
                        </FormLabel>
                        <FormInputHelpText
                          text={translate('SeasonPackUpgradeAllowHelpText')}
                        />
                        <FormInputHelpText
                          text={
                            settings.seasonPackUpgrade.value === 'any'
                              ? translate('SeasonPackUpgradeAllowAnyWarning')
                              : undefined
                          }
                          isWarning={true}
                        />
                        <FormInput
                          type={inputTypes.SELECT}
                          name="seasonPackUpgrade"
                          values={seasonPackUpgradeOptions}
                          onChange={handleInputChange}
                          {...settings.seasonPackUpgrade}
                        />
                      </FormRow>

                      {settings.seasonPackUpgrade.value === 'threshold' ? (
                        <FormRow
                          advancedSettings={showAdvancedSettings}
                          isAdvanced={true}
                          size={sizes.MEDIUM}
                        >
                          <FormLabel>
                            {translate('SeasonPackUpgradeThresholdLabel')}
                          </FormLabel>
                          <FormInputHelpText
                            text={translate(
                              'SeasonPackUpgradeThresholdHelpText'
                            )}
                          />
                          <FormInputHelpText
                            text={translate(
                              'SeasonPackUpgradeThresholdHelpTextExample',
                              {
                                numberEpisodes: 2,
                                totalEpisodes: 8,
                                count: Math.ceil((100 * 2) / 8),
                              }
                            )}
                          />
                          <FormInputHelpText
                            text={translate(
                              'SeasonPackUpgradeThresholdHelpTextExample',
                              {
                                numberEpisodes: 3,
                                totalEpisodes: 12,
                                count: Math.ceil((100 * 3) / 12),
                              }
                            )}
                          />
                          <FormInputHelpText
                            text={translate(
                              'SeasonPackUpgradeThresholdHelpTextExample',
                              {
                                numberEpisodes: 6,
                                totalEpisodes: 24,
                                count: Math.ceil((100 * 6) / 24),
                              }
                            )}
                          />
                          <FormInput
                            type={inputTypes.FLOAT}
                            name="seasonPackUpgradeThreshold"
                            unit="%"
                            step={0.01}
                            min={0}
                            max={100}
                            onChange={handleInputChange}
                            {...settings.seasonPackUpgradeThreshold}
                          />
                        </FormRow>
                      ) : null}
                    </>
                  ) : null}
                </FieldSet>
              ) : null}

              <FieldSet
                legend={translate('FileManagement')}
                caption={translate('FileManagementCaption')}
              >
                <FormRow size={sizes.MEDIUM}>
                  <FormLabel>{translate('UnmonitorDeletedEpisodes')}</FormLabel>
                  <FormInputHelpText
                    text={translate('UnmonitorDeletedEpisodesHelpText')}
                  />
                  <FormInput
                    type={inputTypes.CHECK}
                    name="autoUnmonitorPreviouslyDownloadedEpisodes"
                    onChange={handleInputChange}
                    {...settings.autoUnmonitorPreviouslyDownloadedEpisodes}
                  />
                </FormRow>

                <FormRow
                  advancedSettings={showAdvancedSettings}
                  isAdvanced={true}
                  size={sizes.MEDIUM}
                >
                  <FormLabel>
                    {translate('DownloadPropersAndRepacks')}
                  </FormLabel>
                  <FormInputHelpText
                    text={translate('DownloadPropersAndRepacksHelpText')}
                  />
                  <FormInputHelpText
                    text={translate(
                      'DownloadPropersAndRepacksHelpTextCustomFormat'
                    )}
                  />
                  <FormInputHelpText
                    text={
                      settings.downloadPropersAndRepacks.value === 'doNotPrefer'
                        ? translate('DownloadPropersAndRepacksHelpTextWarning')
                        : undefined
                    }
                    isWarning={true}
                  />
                  <FormInput
                    type={inputTypes.SELECT}
                    name="downloadPropersAndRepacks"
                    values={downloadPropersAndRepacksOptions}
                    onChange={handleInputChange}
                    {...settings.downloadPropersAndRepacks}
                  />
                </FormRow>

                <FormRow
                  advancedSettings={showAdvancedSettings}
                  isAdvanced={true}
                  size={sizes.MEDIUM}
                >
                  <FormLabel>{translate('AnalyseVideoFiles')}</FormLabel>
                  <FormInputHelpText
                    text={translate('AnalyseVideoFilesHelpText')}
                  />
                  <FormInput
                    type={inputTypes.CHECK}
                    name="enableMediaInfo"
                    onChange={handleInputChange}
                    {...settings.enableMediaInfo}
                  />
                </FormRow>

                <FormRow
                  advancedSettings={showAdvancedSettings}
                  isAdvanced={true}
                >
                  <FormLabel>
                    {translate('RescanSeriesFolderAfterRefresh')}
                  </FormLabel>
                  <FormInputHelpText
                    text={translate('RescanAfterRefreshSeriesHelpText')}
                  />
                  <FormInputHelpText
                    text={translate('RescanAfterRefreshHelpTextWarning')}
                    isWarning={true}
                  />
                  <FormInput
                    type={inputTypes.SELECT}
                    name="rescanAfterRefresh"
                    values={rescanAfterRefreshOptions}
                    onChange={handleInputChange}
                    {...settings.rescanAfterRefresh}
                  />
                </FormRow>

                <FormRow
                  advancedSettings={showAdvancedSettings}
                  isAdvanced={true}
                >
                  <FormLabel>{translate('ChangeFileDate')}</FormLabel>
                  <FormInputHelpText
                    text={translate('ChangeFileDateHelpText')}
                  />
                  <FormInput
                    type={inputTypes.SELECT}
                    name="fileDate"
                    values={fileDateOptions}
                    onChange={handleInputChange}
                    {...settings.fileDate}
                  />
                </FormRow>

                <FormRow
                  advancedSettings={showAdvancedSettings}
                  isAdvanced={true}
                >
                  <FormLabel>{translate('RecyclingBin')}</FormLabel>
                  <FormInputHelpText text={translate('RecyclingBinHelpText')} />
                  <FormInput
                    type={inputTypes.PATH}
                    name="recycleBin"
                    includeFiles={false}
                    onChange={handleInputChange}
                    {...settings.recycleBin}
                  />
                </FormRow>

                <FormRow
                  advancedSettings={showAdvancedSettings}
                  isAdvanced={true}
                >
                  <FormLabel>{translate('RecyclingBinCleanup')}</FormLabel>
                  <FormInputHelpText
                    text={translate('RecyclingBinCleanupHelpText')}
                  />
                  <FormInputHelpText
                    text={translate('RecyclingBinCleanupHelpTextWarning')}
                    isWarning={true}
                  />
                  <FormInput
                    type={inputTypes.NUMBER}
                    name="recycleBinCleanupDays"
                    min={0}
                    onChange={handleInputChange}
                    {...settings.recycleBinCleanupDays}
                  />
                </FormRow>
              </FieldSet>

              {showAdvancedSettings && !isWindows ? (
                <FieldSet
                  legend={translate('Permissions')}
                  caption={translate('PermissionsCaption')}
                >
                  <FormRow
                    advancedSettings={showAdvancedSettings}
                    isAdvanced={true}
                    size={sizes.MEDIUM}
                  >
                    <FormLabel>{translate('SetPermissions')}</FormLabel>
                    <FormInputHelpText
                      text={translate('SetPermissionsLinuxHelpText')}
                    />
                    <FormInputHelpText
                      text={translate('SetPermissionsLinuxHelpTextWarning')}
                      isWarning={true}
                    />
                    <FormInput
                      type={inputTypes.CHECK}
                      name="setPermissionsLinux"
                      onChange={handleInputChange}
                      {...settings.setPermissionsLinux}
                    />
                  </FormRow>

                  <FormRow
                    advancedSettings={showAdvancedSettings}
                    isAdvanced={true}
                  >
                    <FormLabel>{translate('ChmodFolder')}</FormLabel>
                    <FormInputHelpText
                      text={translate('ChmodFolderHelpText')}
                    />
                    <FormInputHelpText
                      text={translate('ChmodFolderHelpTextWarning')}
                      isWarning={true}
                    />
                    <FormInput
                      type={inputTypes.UMASK}
                      name="chmodFolder"
                      onChange={handleInputChange}
                      {...settings.chmodFolder}
                    />
                  </FormRow>

                  <FormRow
                    advancedSettings={showAdvancedSettings}
                    isAdvanced={true}
                  >
                    <FormLabel>{translate('ChownGroup')}</FormLabel>
                    <FormInputHelpText text={translate('ChownGroupHelpText')} />
                    <FormInputHelpText
                      text={translate('ChownGroupHelpTextWarning')}
                      isWarning={true}
                    />
                    <FormInput
                      type={inputTypes.TEXT}
                      name="chownGroup"
                      onChange={handleInputChange}
                      {...settings.chownGroup}
                    />
                  </FormRow>
                </FieldSet>
              ) : null}
            </Form>
          ) : null}

          <FieldSet
            legend={translate('RootFolders')}
            caption={translate('RootFoldersCaption')}
          >
            <RootFolders />
            <AddRootFolder />
          </FieldSet>
        </div>
      </PageContentBody>
    </SettingsPage>
  );
}

export default MediaManagement;
