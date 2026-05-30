import React, { useCallback, useRef, useState } from 'react';
import PageContentBody from 'Components/Page/PageContentBody';
import { OverflowDivider } from 'Components/Page/Toolbar/Overflow';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import ToolbarItem from 'Components/Page/Toolbar/ToolbarItem';
import { icons } from 'Helpers/Props';
import SettingsPage from 'Settings/SettingsPage';
import {
  SaveCallback,
  SettingsStateChange,
} from 'typings/Settings/SettingsState';
import translate from 'Utilities/String/translate';
import Indexers from './Indexers/Indexers';
import ManageIndexersModal from './Indexers/Manage/ManageIndexersModal';
import IndexerOptions from './Options/IndexerOptions';
import { useTestAllIndexers } from './useIndexers';

function IndexerSettings() {
  const { isTestingAllIndexers, testAllIndexers } = useTestAllIndexers();

  const saveOptions = useRef<() => void>();

  const [isSaving, setIsSaving] = useState(false);
  const [hasPendingChanges, setHasPendingChanges] = useState(false);
  const [isManageIndexersModalOpen, setIsManageIndexersModalOpen] =
    useState(false);

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

  const handleManageIndexersPress = useCallback(() => {
    setIsManageIndexersModalOpen(true);
  }, []);

  const handleManageIndexersModalClose = useCallback(() => {
    setIsManageIndexersModalOpen(false);
  }, []);

  const handleSavePress = useCallback(() => {
    saveOptions.current?.();
  }, []);

  const handleTestAllIndexersPress = useCallback(() => {
    testAllIndexers();
  }, [testAllIndexers]);

  return (
    <SettingsPage
      title={translate('IndexerSettings')}
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
            label={translate('TestAllIndexers')}
            iconName={icons.TEST}
            isSpinning={isTestingAllIndexers}
            onPress={handleTestAllIndexersPress}
          />

          <ToolbarItem
            id="manage"
            priority={1}
            groupId="extras"
            label={translate('ManageIndexers')}
            iconName={icons.MANAGE}
            onPress={handleManageIndexersPress}
          />
        </>
      }
      onSavePress={handleSavePress}
    >
      <PageContentBody>
        <Indexers />

        <IndexerOptions
          setChildSave={handleSetChildSave}
          onChildStateChange={handleChildStateChange}
        />

        <ManageIndexersModal
          isOpen={isManageIndexersModalOpen}
          onModalClose={handleManageIndexersModalClose}
        />
      </PageContentBody>
    </SettingsPage>
  );
}

export default IndexerSettings;
