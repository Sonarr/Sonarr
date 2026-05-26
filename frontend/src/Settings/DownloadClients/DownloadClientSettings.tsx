import React, { useCallback, useRef, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import PageContentBody from 'Components/Page/PageContentBody';
import { OverflowDivider } from 'Components/Page/Toolbar/Overflow';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import ToolbarItem from 'Components/Page/Toolbar/ToolbarItem';
import { icons } from 'Helpers/Props';
import SettingsPage from 'Settings/SettingsPage';
import { testAllDownloadClients } from 'Store/Actions/settingsActions';
import {
  SaveCallback,
  SettingsStateChange,
} from 'typings/Settings/SettingsState';
import translate from 'Utilities/String/translate';
import DownloadClients from './DownloadClients/DownloadClients';
import ManageDownloadClientsModal from './DownloadClients/Manage/ManageDownloadClientsModal';
import DownloadClientOptions from './Options/DownloadClientOptions';
import RemotePathMappings from './RemotePathMappings/RemotePathMappings';

function DownloadClientSettings() {
  const dispatch = useDispatch();
  const isTestingAll = useSelector(
    (state: AppState) => state.settings.downloadClients.isTestingAll
  );

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
    dispatch(testAllDownloadClients());
  }, [dispatch]);

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
            isSpinning={isTestingAll}
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
    </SettingsPage>
  );
}

export default DownloadClientSettings;
