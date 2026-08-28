import React, { useCallback, useEffect, useRef, useState } from 'react';
import CommandNames from 'Commands/CommandNames';
import { useCommandExecuting } from 'Commands/useCommands';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { kinds } from 'Helpers/Props';
import SettingsToolbar from 'Settings/SettingsToolbar';
import { useIsWindowsService } from 'System/Status/useSystemStatus';
import { useRestart } from 'System/useSystem';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import AnalyticSettings from './AnalyticSettings';
import BackupSettings from './BackupSettings';
import HostSettings from './HostSettings';
import LoggingSettings from './LoggingSettings';
import ProxySettings from './ProxySettings';
import SecuritySettings from './SecuritySettings';
import UpdateSettings from './UpdateSettings';
import { useManageGeneralSettings } from './useGeneralSettings';

const requiresRestartKeys = [
  'bindAddress',
  'port',
  'urlBase',
  'allowedHosts',
  'trustedNetworks',
  'instanceName',
  'enableSsl',
  'sslPort',
  'sslCertHash',
  'sslCertPassword',
];

function GeneralSettings() {
  const isWindowsService = useIsWindowsService();
  const { mutate: restart } = useRestart();
  const isResettingApiKey = useCommandExecuting(CommandNames.ResetApiKey);

  const {
    settings,
    isFetching,
    isFetched,
    error,
    updateSetting,
    saveSettings,
    isSaving,
    saveError,
    hasPendingChanges,
    pendingChanges,
    validationErrors,
    validationWarnings,
  } = useManageGeneralSettings();

  const wasResettingApiKey = usePrevious(isResettingApiKey);
  const wasSaving = usePrevious(isSaving);
  const isRestartRequired = useRef(false);

  const [isRestartRequiredModalOpen, setIsRestartRequiredModalOpen] =
    useState(false);

  const handleInputChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error input change events aren't typed
      updateSetting(change.name, change.value);
    },
    [updateSetting]
  );

  const handleSavePress = useCallback(() => {
    isRestartRequired.current = Object.keys(pendingChanges ?? {}).some(
      (key) => {
        return requiresRestartKeys.includes(key);
      }
    );

    saveSettings();
  }, [pendingChanges, saveSettings]);

  const handleConfirmRestart = useCallback(() => {
    setIsRestartRequiredModalOpen(false);
    restart();
  }, [restart]);

  const handleCloseRestartRequiredModalOpen = useCallback(() => {
    setIsRestartRequiredModalOpen(false);
  }, []);

  useEffect(() => {
    if (!isSaving && wasSaving && !saveError && isRestartRequired.current) {
      isRestartRequired.current = false;
      setIsRestartRequiredModalOpen(true);
    }
  }, [isSaving, wasSaving, saveError]);

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
        {isFetching && !isFetched ? <LoadingIndicator /> : null}

        {!isFetching && error ? (
          <Alert kind={kinds.DANGER}>
            {translate('GeneralSettingsLoadError')}
          </Alert>
        ) : null}

        {settings && isFetched && !error ? (
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
              allowedHosts={settings.allowedHosts}
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
              trustedNetworks={settings.trustedNetworks}
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
