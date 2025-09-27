import React, { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import { EnhancedSelectInputValue } from 'Components/Form/Select/EnhancedSelectInput';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import useShowAdvancedSettings from 'Helpers/Hooks/useShowAdvancedSettings';
import { inputTypes, kinds, sizes } from 'Helpers/Props';
import RootFolders from 'RootFolder/RootFolders';
import SettingsToolbar from 'Settings/SettingsToolbar';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import {
  fetchMediaManagementSettings,
  saveMediaManagementSettings,
  saveNamingSettings,
  setMediaManagementSettingsValue,
} from 'Store/Actions/settingsActions';
import createSettingsSectionSelector from 'Store/Selectors/createSettingsSectionSelector';
import useIsWindows from 'System/useIsWindows';
import { InputChanged } from 'typings/inputs';
import isEmpty from 'Utilities/Object/isEmpty';
import translate from 'Utilities/String/translate';
import Naming from './Naming/Naming';
import AddRootFolder from './RootFolder/AddRootFolder';

const SECTION = 'mediaManagement';

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
  const dispatch = useDispatch();
  const showAdvancedSettings = useShowAdvancedSettings();
  const hasNamingPendingChanges = !isEmpty(
    useSelector((state: AppState) => state.settings.naming.pendingChanges)
  );
  const isWindows = useIsWindows();
  const {
    isFetching,
    isPopulated,
    isSaving,
    error,
    settings,
    hasSettings,
    hasPendingChanges,
    validationErrors,
    validationWarnings,
  } = useSelector(createSettingsSectionSelector(SECTION));

  const handleSavePress = useCallback(() => {
    dispatch(saveMediaManagementSettings());
    dispatch(saveNamingSettings());
  }, [dispatch]);

  const handleInputChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error - actions are not typed
      dispatch(setMediaManagementSettingsValue(change));
    },
    [dispatch]
  );

  useEffect(() => {
    dispatch(fetchMediaManagementSettings());

    return () => {
      dispatch(clearPendingChanges({ section: `settings.${SECTION}` }));
    };
  }, [dispatch]);

  return (
    <PageContent title={translate('MediaManagementSettings')}>
      <SettingsToolbar
        isSaving={isSaving}
        hasPendingChanges={hasNamingPendingChanges || hasPendingChanges}
        onSavePress={handleSavePress}
      />

      <PageContentBody>
        <Naming />

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
              <FieldSet legend={translate('Folders')}>
                <FormGroup
                  advancedSettings={showAdvancedSettings}
                  isAdvanced={true}
                  size={sizes.MEDIUM}
                >
                  <FormLabel>{translate('CreateEmptySeriesFolders')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.CHECK}
                    name="createEmptySeriesFolders"
                    helpText={translate('CreateEmptySeriesFoldersHelpText')}
                    onChange={handleInputChange}
                    {...settings.createEmptySeriesFolders}
                  />
                </FormGroup>

                <FormGroup
                  advancedSettings={showAdvancedSettings}
                  isAdvanced={true}
                  size={sizes.MEDIUM}
                >
                  <FormLabel>{translate('DeleteEmptyFolders')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.CHECK}
                    name="deleteEmptyFolders"
                    helpText={translate('DeleteEmptySeriesFoldersHelpText')}
                    onChange={handleInputChange}
                    {...settings.deleteEmptyFolders}
                  />
                </FormGroup>
              </FieldSet>
            ) : null}

            {showAdvancedSettings ? (
              <FieldSet legend={translate('Importing')}>
                <FormGroup
                  advancedSettings={showAdvancedSettings}
                  isAdvanced={true}
                  size={sizes.SMALL}
                >
                  <FormLabel>{translate('EpisodeTitleRequired')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.SELECT}
                    name="episodeTitleRequired"
                    helpText={translate('EpisodeTitleRequiredHelpText')}
                    values={episodeTitleRequiredOptions}
                    onChange={handleInputChange}
                    {...settings.episodeTitleRequired}
                  />
                </FormGroup>

                <FormGroup
                  advancedSettings={showAdvancedSettings}
                  isAdvanced={true}
                  size={sizes.MEDIUM}
                >
                  <FormLabel>{translate('SkipFreeSpaceCheck')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.CHECK}
                    name="skipFreeSpaceCheckWhenImporting"
                    helpText={translate('SkipFreeSpaceCheckHelpText')}
                    onChange={handleInputChange}
                    {...settings.skipFreeSpaceCheckWhenImporting}
                  />
                </FormGroup>

                <FormGroup
                  advancedSettings={showAdvancedSettings}
                  isAdvanced={true}
                  size={sizes.MEDIUM}
                >
                  <FormLabel>{translate('MinimumFreeSpace')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.NUMBER}
                    unit="MB"
                    name="minimumFreeSpaceWhenImporting"
                    helpText={translate('MinimumFreeSpaceHelpText')}
                    onChange={handleInputChange}
                    {...settings.minimumFreeSpaceWhenImporting}
                  />
                </FormGroup>

                <FormGroup
                  advancedSettings={showAdvancedSettings}
                  isAdvanced={true}
                  size={sizes.MEDIUM}
                >
                  <FormLabel>
                    {translate('UseHardlinksInsteadOfCopy')}
                  </FormLabel>

                  <FormInputGroup
                    type={inputTypes.CHECK}
                    name="copyUsingHardlinks"
                    helpText={translate('CopyUsingHardlinksSeriesHelpText')}
                    helpTextWarning={translate(
                      'CopyUsingHardlinksHelpTextWarning'
                    )}
                    onChange={handleInputChange}
                    {...settings.copyUsingHardlinks}
                  />
                </FormGroup>

                <FormGroup
                  advancedSettings={showAdvancedSettings}
                  isAdvanced={true}
                  size={sizes.MEDIUM}
                >
                  <FormLabel>{translate('ImportUsingScript')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.CHECK}
                    name="useScriptImport"
                    helpText={translate('ImportUsingScriptHelpText')}
                    onChange={handleInputChange}
                    {...settings.useScriptImport}
                  />
                </FormGroup>

                {settings.useScriptImport.value ? (
                  <FormGroup
                    advancedSettings={showAdvancedSettings}
                    isAdvanced={true}
                  >
                    <FormLabel>{translate('ImportScriptPath')}</FormLabel>

                    <FormInputGroup
                      type={inputTypes.PATH}
                      includeFiles={true}
                      name="scriptImportPath"
                      helpText={translate('ImportScriptPathHelpText')}
                      onChange={handleInputChange}
                      {...settings.scriptImportPath}
                    />
                  </FormGroup>
                ) : null}

                <FormGroup size={sizes.MEDIUM}>
                  <FormLabel>{translate('ImportExtraFiles')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.CHECK}
                    name="importExtraFiles"
                    helpText={translate('ImportExtraFilesEpisodeHelpText')}
                    onChange={handleInputChange}
                    {...settings.importExtraFiles}
                  />
                </FormGroup>

                {settings.importExtraFiles.value ? (
                  <FormGroup
                    advancedSettings={showAdvancedSettings}
                    isAdvanced={true}
                  >
                    <FormLabel>{translate('ImportExtraFiles')}</FormLabel>

                    <FormInputGroup
                      type={inputTypes.TEXT}
                      name="extraFileExtensions"
                      helpTexts={[
                        translate('ExtraFileExtensionsHelpText'),
                        translate('ExtraFileExtensionsHelpTextsExamples'),
                      ]}
                      onChange={handleInputChange}
                      {...settings.extraFileExtensions}
                    />
                  </FormGroup>
                ) : null}

                <FormGroup
                  advancedSettings={showAdvancedSettings}
                  isAdvanced={true}
                >
                  <FormLabel>{translate('UserRejectedExtensions')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.TEXT}
                    name="userRejectedExtensions"
                    helpTexts={[
                      translate('UserRejectedExtensionsHelpText'),
                      translate('UserRejectedExtensionsTextsExamples'),
                    ]}
                    onChange={handleInputChange}
                    {...settings.userRejectedExtensions}
                  />
                </FormGroup>

                {showAdvancedSettings && (
                  <>
                    <FormGroup
                      advancedSettings={showAdvancedSettings}
                      isAdvanced={true}
                      size={sizes.MEDIUM}
                    >
                      <FormLabel>
                        {translate('SeasonPackUpgradeAllowLabel')}
                      </FormLabel>
                      <FormInputGroup
                        type={inputTypes.SELECT}
                        name="seasonPackUpgrade"
                        helpText={translate('SeasonPackUpgradeAllowHelpText')}
                        helpTextWarning={
                          settings.seasonPackUpgrade.value === 'any'
                            ? translate('SeasonPackUpgradeAllowAnyWarning')
                            : undefined
                        }
                        values={seasonPackUpgradeOptions}
                        onChange={handleInputChange}
                        {...settings.seasonPackUpgrade}
                      />
                    </FormGroup>

                    {settings.seasonPackUpgrade.value === 'threshold' && (
                      <FormGroup
                        advancedSettings={showAdvancedSettings}
                        isAdvanced={true}
                        size={sizes.MEDIUM}
                      >
                        <FormLabel>
                          {translate('SeasonPackUpgradeThresholdLabel')}
                        </FormLabel>
                        <FormInputGroup
                          type={inputTypes.FLOAT}
                          name="seasonPackUpgradeThreshold"
                          unit="%"
                          step={0.01}
                          min={0}
                          max={100}
                          helpTexts={[
                            translate('SeasonPackUpgradeThresholdHelpText'),
                            translate(
                              'SeasonPackUpgradeThresholdHelpTextExample',
                              {
                                numberEpisodes: 2,
                                totalEpisodes: 8,
                                count: Math.ceil((100 * 2) / 8),
                              }
                            ),
                            translate(
                              'SeasonPackUpgradeThresholdHelpTextExample',
                              {
                                numberEpisodes: 3,
                                totalEpisodes: 12,
                                count: Math.ceil((100 * 3) / 12),
                              }
                            ),
                            translate(
                              'SeasonPackUpgradeThresholdHelpTextExample',
                              {
                                numberEpisodes: 6,
                                totalEpisodes: 24,
                                count: Math.ceil((100 * 6) / 24),
                              }
                            ),
                          ]}
                          onChange={handleInputChange}
                          {...settings.seasonPackUpgradeThreshold}
                        />
                      </FormGroup>
                    )}
                  </>
                )}
              </FieldSet>
            ) : null}

            <FieldSet legend={translate('FileManagement')}>
              <FormGroup size={sizes.MEDIUM}>
                <FormLabel>{translate('UnmonitorDeletedEpisodes')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="autoUnmonitorPreviouslyDownloadedEpisodes"
                  helpText={translate('UnmonitorDeletedEpisodesHelpText')}
                  onChange={handleInputChange}
                  {...settings.autoUnmonitorPreviouslyDownloadedEpisodes}
                />
              </FormGroup>

              <FormGroup
                advancedSettings={showAdvancedSettings}
                isAdvanced={true}
                size={sizes.MEDIUM}
              >
                <FormLabel>{translate('DownloadPropersAndRepacks')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="downloadPropersAndRepacks"
                  helpTexts={[
                    translate('DownloadPropersAndRepacksHelpText'),
                    translate('DownloadPropersAndRepacksHelpTextCustomFormat'),
                  ]}
                  helpTextWarning={
                    settings.downloadPropersAndRepacks.value === 'doNotPrefer'
                      ? translate('DownloadPropersAndRepacksHelpTextWarning')
                      : undefined
                  }
                  values={downloadPropersAndRepacksOptions}
                  onChange={handleInputChange}
                  {...settings.downloadPropersAndRepacks}
                />
              </FormGroup>

              <FormGroup
                advancedSettings={showAdvancedSettings}
                isAdvanced={true}
                size={sizes.MEDIUM}
              >
                <FormLabel>{translate('AnalyseVideoFiles')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="enableMediaInfo"
                  helpText={translate('AnalyseVideoFilesHelpText')}
                  onChange={handleInputChange}
                  {...settings.enableMediaInfo}
                />
              </FormGroup>

              <FormGroup
                advancedSettings={showAdvancedSettings}
                isAdvanced={true}
              >
                <FormLabel>
                  {translate('RescanSeriesFolderAfterRefresh')}
                </FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="rescanAfterRefresh"
                  helpText={translate('RescanAfterRefreshSeriesHelpText')}
                  helpTextWarning={translate(
                    'RescanAfterRefreshHelpTextWarning'
                  )}
                  values={rescanAfterRefreshOptions}
                  onChange={handleInputChange}
                  {...settings.rescanAfterRefresh}
                />
              </FormGroup>

              <FormGroup
                advancedSettings={showAdvancedSettings}
                isAdvanced={true}
              >
                <FormLabel>{translate('ChangeFileDate')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="fileDate"
                  helpText={translate('ChangeFileDateHelpText')}
                  values={fileDateOptions}
                  onChange={handleInputChange}
                  {...settings.fileDate}
                />
              </FormGroup>

              <FormGroup
                advancedSettings={showAdvancedSettings}
                isAdvanced={true}
              >
                <FormLabel>{translate('RecyclingBin')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.PATH}
                  name="recycleBin"
                  helpText={translate('RecyclingBinHelpText')}
                  includeFiles={false}
                  onChange={handleInputChange}
                  {...settings.recycleBin}
                />
              </FormGroup>

              <FormGroup
                advancedSettings={showAdvancedSettings}
                isAdvanced={true}
              >
                <FormLabel>{translate('RecyclingBinCleanup')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.NUMBER}
                  name="recycleBinCleanupDays"
                  helpText={translate('RecyclingBinCleanupHelpText')}
                  helpTextWarning={translate(
                    'RecyclingBinCleanupHelpTextWarning'
                  )}
                  min={0}
                  onChange={handleInputChange}
                  {...settings.recycleBinCleanupDays}
                />
              </FormGroup>
            </FieldSet>

            {showAdvancedSettings && !isWindows ? (
              <FieldSet legend={translate('Permissions')}>
                <FormGroup
                  advancedSettings={showAdvancedSettings}
                  isAdvanced={true}
                  size={sizes.MEDIUM}
                >
                  <FormLabel>{translate('SetPermissions')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.CHECK}
                    name="setPermissionsLinux"
                    helpText={translate('SetPermissionsLinuxHelpText')}
                    helpTextWarning={translate(
                      'SetPermissionsLinuxHelpTextWarning'
                    )}
                    onChange={handleInputChange}
                    {...settings.setPermissionsLinux}
                  />
                </FormGroup>

                <FormGroup
                  advancedSettings={showAdvancedSettings}
                  isAdvanced={true}
                >
                  <FormLabel>{translate('ChmodFolder')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.UMASK}
                    name="chmodFolder"
                    helpText={translate('ChmodFolderHelpText')}
                    helpTextWarning={translate('ChmodFolderHelpTextWarning')}
                    onChange={handleInputChange}
                    {...settings.chmodFolder}
                  />
                </FormGroup>

                <FormGroup
                  advancedSettings={showAdvancedSettings}
                  isAdvanced={true}
                >
                  <FormLabel>{translate('ChownGroup')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.TEXT}
                    name="chownGroup"
                    helpText={translate('ChownGroupHelpText')}
                    helpTextWarning={translate('ChownGroupHelpTextWarning')}
                    onChange={handleInputChange}
                    {...settings.chownGroup}
                  />
                </FormGroup>
              </FieldSet>
            ) : null}
          </Form>
        ) : null}

        <FieldSet legend={translate('RootFolders')}>
          <RootFolders />
          <AddRootFolder />
        </FieldSet>
      </PageContentBody>
    </PageContent>
  );
}

export default MediaManagement;
