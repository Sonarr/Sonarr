import React, { useCallback, useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import * as commandNames from 'Commands/commandNames';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { kinds } from 'Helpers/Props';
import SettingsToolbar from 'Settings/SettingsToolbar';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import {
  fetchGeneralSettings,
  saveGeneralSettings,
  setGeneralSettingsValue,
} from 'Store/Actions/settingsActions';
import { restart } from 'Store/Actions/systemActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import createSettingsSectionSelector from 'Store/Selectors/createSettingsSectionSelector';
import useIsWindowsService from 'System/useIsWindowsService';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import AnalyticSettings from './AnalyticSettings';
import BackupSettings from './BackupSettings';
import HostSettings from './HostSettings';
import LoggingSettings from './LoggingSettings';
import ProxySettings from './ProxySettings';
import SecuritySettings from './SecuritySettings';
import UpdateSettings from './UpdateSettings';

const SECTION = 'general';

const requiresRestartKeys = [
  'bindAddress',
  'port',
  'urlBase',
  'instanceName',
  'enableSsl',
  'sslPort',
  'sslCertHash',
  'sslCertPassword',
];

function GeneralSettings() {
  const dispatch = useDispatch();
  const isWindowsService = useIsWindowsService();
  const isResettingApiKey = useSelector(
    createCommandExecutingSelector(commandNames.RESET_API_KEY)
  );

  const {
    isFetching,
    isPopulated,
    isSaving,
    error,
    saveError,
    settings,
    hasSettings,
    hasPendingChanges,
    pendingChanges,
    validationErrors,
    validationWarnings,
  } = useSelector(createSettingsSectionSelector(SECTION));

  const wasResettingApiKey = usePrevious(isResettingApiKey);
  const wasSaving = usePrevious(isSaving);
  const previousPendingChanges = usePrevious(pendingChanges);

  const [isRestartRequiredModalOpen, setIsRestartRequiredModalOpen] =
    useState(false);

  const handleInputChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error - actions aren't typed
      dispatch(setGeneralSettingsValue(change));
    },
    [dispatch]
  );

  const handleSavePress = useCallback(() => {
    dispatch(saveGeneralSettings());
  }, [dispatch]);

  const handleConfirmRestart = useCallback(() => {
    setIsRestartRequiredModalOpen(false);
    dispatch(restart());
  }, [dispatch]);

  const handleCloseRestartRequiredModalOpen = useCallback(() => {
    setIsRestartRequiredModalOpen(false);
  }, []);

  useEffect(() => {
    dispatch(fetchGeneralSettings());

    return () => {
      dispatch(clearPendingChanges({ section: `settings.${SECTION}` }));
    };
  }, [dispatch]);

  useEffect(() => {
    if (!isResettingApiKey && wasResettingApiKey) {
      dispatch(fetchGeneralSettings());
    }
  }, [isResettingApiKey, wasResettingApiKey, dispatch]);

  useEffect(() => {
    const isRestartedRequired =
      previousPendingChanges &&
      Object.keys(previousPendingChanges).some((key) => {
        return requiresRestartKeys.includes(key);
      });

    if (!isSaving && wasSaving && !saveError && isRestartedRequired) {
      setIsRestartRequiredModalOpen(true);
    }
  }, [isSaving, wasSaving, saveError, previousPendingChanges]);

  useEffect(() => {
    if (!isResettingApiKey && wasResettingApiKey) {
      setIsRestartRequiredModalOpen(true);
    }
  }, [isResettingApiKey, wasResettingApiKey]);

  return (
    <PageContent title={translate('GeneralSettings')}>
      <SettingsToolbar
        hasPendingChanges={hasPendingChanges}
        isSaving={isSaving}
        onSavePress={handleSavePress}
      />

      <PageContentBody>
        {isFetching && !isPopulated ? <LoadingIndicator /> : null}

        {!isFetching && error ? (
          <Alert kind={kinds.DANGER}>
            {translate('GeneralSettingsLoadError')}
          </Alert>
        ) : null}

        {hasSettings && isPopulated && !error ? (
          <Form
            id="generalSettings"
            validationErrors={validationErrors}
            validationWarnings={validationWarnings}
          >
            <HostSettings
              bindAddress={settings.bindAddress}
              port={settings.port}
              urlBase={settings.urlBase}
              instanceName={settings.instanceName}
              applicationUrl={settings.applicationUrl}
              enableSsl={settings.enableSsl}
              sslPort={settings.sslPort}
              sslCertPath={settings.sslCertPath}
              sslKeyPath={settings.sslKeyPath}
              sslCertPassword={settings.sslCertPassword}
              launchBrowser={settings.launchBrowser}
              onInputChange={handleInputChange}
            />

            <SecuritySettings
              authenticationMethod={settings.authenticationMethod}
              authenticationRequired={settings.authenticationRequired}
              username={settings.username}
              password={settings.password}
              passwordConfirmation={settings.passwordConfirmation}
              apiKey={settings.apiKey}
              certificateValidation={settings.certificateValidation}
              isResettingApiKey={isResettingApiKey}
              onInputChange={handleInputChange}
            />

            <ProxySettings
              proxyEnabled={settings.proxyEnabled}
              proxyType={settings.proxyType}
              proxyHostname={settings.proxyHostname}
              proxyPort={settings.proxyPort}
              proxyUsername={settings.proxyUsername}
              proxyPassword={settings.proxyPassword}
              proxyBypassFilter={settings.proxyBypassFilter}
              proxyBypassLocalAddresses={settings.proxyBypassLocalAddresses}
              onInputChange={handleInputChange}
            />

            <LoggingSettings
              logLevel={settings.logLevel}
              logSizeLimit={settings.logSizeLimit}
              onInputChange={handleInputChange}
            />

            <AnalyticSettings
              analyticsEnabled={settings.analyticsEnabled}
              onInputChange={handleInputChange}
            />

            <UpdateSettings
              branch={settings.branch}
              updateAutomatically={settings.updateAutomatically}
              updateMechanism={settings.updateMechanism}
              updateScriptPath={settings.updateScriptPath}
              onInputChange={handleInputChange}
            />

            <BackupSettings
              backupFolder={settings.backupFolder}
              backupInterval={settings.backupInterval}
              backupRetention={settings.backupRetention}
              onInputChange={handleInputChange}
            />
          </Form>
        ) : null}
      </PageContentBody>

      <ConfirmModal
        isOpen={isRestartRequiredModalOpen}
        kind={kinds.DANGER}
        title={translate('RestartSonarr')}
        message={`${translate('RestartRequiredToApplyChanges')} ${
          isWindowsService ? translate('RestartRequiredWindowsService') : ''
        }`}
        cancelLabel={translate('RestartLater')}
        confirmLabel={translate('RestartNow')}
        onConfirm={handleConfirmRestart}
        onCancel={handleCloseRestartRequiredModalOpen}
      />
    </PageContent>
  );
}

export default GeneralSettings;
