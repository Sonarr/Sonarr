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
import ImportListExclusions from './ImportListExclusions/ImportListExclusions';
import ImportLists from './ImportLists/ImportLists';
import ManageImportListsModal from './ImportLists/Manage/ManageImportListsModal';
import { useTestAllImportLists } from './ImportLists/useImportLists';
import ImportListOptions from './Options/ImportListOptions';

function ImportListSettings() {
  const { isTestingAllImportLists, testAllImportLists } =
    useTestAllImportLists();

  const saveOptions = useRef<() => void>();

  const [isSaving, setIsSaving] = useState(false);
  const [hasPendingChanges, setHasPendingChanges] = useState(false);
  const [isManageImportListsModalOpen, setIsManageImportListsModalOpen] =
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

  const handleManageImportListsPress = useCallback(() => {
    setIsManageImportListsModalOpen(true);
  }, []);

  const handleManageImportListsModalClose = useCallback(() => {
    setIsManageImportListsModalOpen(false);
  }, []);

  const handleSavePress = useCallback(() => {
    saveOptions.current?.();
  }, []);

  const handleTestAllImportListsPress = useCallback(() => {
    testAllImportLists();
  }, [testAllImportLists]);

  return (
    <SettingsPage
      title={translate('ImportListSettings')}
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
            label={translate('TestAllLists')}
            iconName={icons.TEST}
            isSpinning={isTestingAllImportLists}
            onPress={handleTestAllImportListsPress}
          />

          <ToolbarItem
            id="manage"
            priority={1}
            groupId="extras"
            label={translate('ManageLists')}
            iconName={icons.MANAGE}
            onPress={handleManageImportListsPress}
          />
        </>
      }
      onSavePress={handleSavePress}
    >
      <PageContentBody>
        <ImportLists />

        <ImportListOptions
          setChildSave={handleSetChildSave}
          onChildStateChange={handleChildStateChange}
        />

        <ImportListExclusions />

        <ManageImportListsModal
          isOpen={isManageImportListsModalOpen}
          onModalClose={handleManageImportListsModalClose}
        />
      </PageContentBody>
    </SettingsPage>
  );
}

export default ImportListSettings;
