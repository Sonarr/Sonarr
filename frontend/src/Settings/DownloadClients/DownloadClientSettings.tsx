import React, { useCallback, useRef, useState } from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import { icons } from 'Helpers/Props';
import SettingsToolbar from 'Settings/SettingsToolbar';
import {
  SaveCallback,
  SettingsStateChange,
} from 'typings/Settings/SettingsState';
import translate from 'Utilities/String/translate';
import DownloadClients from './DownloadClients/DownloadClients';
import ManageDownloadClientsModal from './DownloadClients/Manage/ManageDownloadClientsModal';
import { useTestAllDownloadClients } from './DownloadClients/useDownloadClients';
import DownloadClientOptions from './Options/DownloadClientOptions';
import RemotePathMappings from './RemotePathMappings/RemotePathMappings';

function DownloadClientSettings() {
  const { testAllDownloadClients, isTestingAllDownloadClients } =
    useTestAllDownloadClients();

  const saveOptions = useRef<() => void>();

  const [isSaving, setIsSaving] = useState(false);
  const [hasPendingChanges, setHasPendingChanges] = useState(false);
  const [
    isManageDownloadClientsModalOpen,
    setIsManageDownloadClientsModalOpen,
  ] = useState(false);

  const handleSetChildSave = useCallback((saveCallback: SaveCallback) => {
    saveOptions.current = saveCallback;
  }, []);

  const handleChildStateChange = useCallback(
    ({ isSaving, hasPendingChanges }: SettingsStateChange) => {
      setIsSaving(isSaving);
      setHasPendingChanges(hasPendingChanges);
    },
    []
  );

  const handleManageDownloadClientsPress = useCallback(() => {
    setIsManageDownloadClientsModalOpen(true);
  }, []);

  const handleManageDownloadClientsModalClose = useCallback(() => {
    setIsManageDownloadClientsModalOpen(false);
  }, []);

  const handleSavePress = useCallback(() => {
    saveOptions.current?.();
  }, []);

  const handleTestAllClientsPress = useCallback(() => {
    testAllDownloadClients();
  }, [testAllDownloadClients]);

  return (
    <PageContent title={translate('DownloadClientSettings')}>
      <SettingsToolbar
        isSaving={isSaving}
        hasPendingChanges={hasPendingChanges}
        additionalButtons={
          <>
            <PageToolbarSeparator />

            <PageToolbarButton
              label={translate('TestAllClients')}
              iconName={icons.TEST}
              isSpinning={isTestingAllDownloadClients}
              onPress={handleTestAllClientsPress}
            />

            <PageToolbarButton
              label={translate('ManageClients')}
              iconName={icons.MANAGE}
              onPress={handleManageDownloadClientsPress}
            />
          </>
        }
        onSavePress={handleSavePress}
      />

      <PageContentBody>
        <DownloadClients />

        <DownloadClientOptions
          setChildSave={handleSetChildSave}
          onChildStateChange={handleChildStateChange}
        />

        <RemotePathMappings />

        <ManageDownloadClientsModal
          isOpen={isManageDownloadClientsModalOpen}
          onModalClose={handleManageDownloadClientsModalClose}
        />
      </PageContentBody>
    </PageContent>
  );
}

export default DownloadClientSettings;
