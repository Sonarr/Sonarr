import React, { useCallback, useRef, useState } from 'react';
import PageContentBody from 'Components/Page/PageContentBody';
import PageHeading from 'Components/Page/PageHeading';
import { OverflowDivider } from 'Components/Page/Toolbar/Overflow';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import ToolbarItem from 'Components/Page/Toolbar/ToolbarItem';
import SectionHeading from 'Components/SectionHeading';
import { icons } from 'Helpers/Props';
import settingsStyles from 'Settings/Settings.css';
import SettingsPage from 'Settings/SettingsPage';
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

  const handleTestAllDownloadClientsPress = useCallback(() => {
    testAllDownloadClients();
  }, [testAllDownloadClients]);

  return (
    <SettingsPage
      title={translate('DownloadClientSettings')}
      isSaving={isSaving}
      hasPendingChanges={hasPendingChanges}
      toolbarChildren={
        <>
          <OverflowDivider groupId="extras">
            <PageToolbarSeparator />
          </OverflowDivider>

          <ToolbarItem
            id="test-all"
            priority={1}
            groupId="extras"
            label={translate('TestAllClients')}
            iconName={icons.TEST}
            isSpinning={isTestingAllDownloadClients}
            onPress={handleTestAllDownloadClientsPress}
          />

          <ToolbarItem
            id="manage"
            priority={1}
            groupId="extras"
            label={translate('ManageClients')}
            iconName={icons.MANAGE}
            onPress={handleManageDownloadClientsPress}
          />
        </>
      }
      onSavePress={handleSavePress}
    >
      <PageContentBody>
        <div className={settingsStyles.section}>
          <PageHeading
            scope={translate('Settings')}
            title={translate('DownloadClients')}
          />

          <div className={settingsStyles.pageSection}>
            <SectionHeading
              title={translate('DownloadClients')}
              description={translate('DownloadClientsSectionDescription')}
            />

            <DownloadClients />
          </div>

          <DownloadClientOptions
            setChildSave={handleSetChildSave}
            onChildStateChange={handleChildStateChange}
          />

          <RemotePathMappings />

          <ManageDownloadClientsModal
            isOpen={isManageDownloadClientsModalOpen}
            onModalClose={handleManageDownloadClientsModalClose}
          />
        </div>
      </PageContentBody>
    </SettingsPage>
  );
}

export default DownloadClientSettings;
