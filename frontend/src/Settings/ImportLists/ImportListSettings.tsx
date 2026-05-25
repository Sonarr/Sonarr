import React, { useCallback, useMemo, useRef, useState } from 'react';
import PageContentBody from 'Components/Page/PageContentBody';
import { OverflowDivider } from 'Components/Page/Toolbar/Overflow';
import { type MoreMenuItem } from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
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

  const moreMenuItems = useMemo<MoreMenuItem[]>(
    () => [
      {
        id: 'test-all',
        label: translate('TestAllLists'),
        iconName: icons.TEST,
        isSpinning: isTestingAllImportLists,
        onPress: handleTestAllImportListsPress,
      },
      {
        id: 'manage',
        label: translate('ManageLists'),
        iconName: icons.MANAGE,
        onPress: handleManageImportListsPress,
      },
    ],
    [
      isTestingAllImportLists,
      handleTestAllImportListsPress,
      handleManageImportListsPress,
    ]
  );

  return (
    <SettingsPage
      title={translate('ImportListSettings')}
      isSaving={isSaving}
      hasPendingChanges={hasPendingChanges}
      moreMenuItems={moreMenuItems}
      toolbarChildren={
        <>
          <OverflowDivider groupId="extras">
            <PageToolbarSeparator />
          </OverflowDivider>

          <ToolbarItem id="test-all" priority={1} groupId="extras">
            <PageToolbarButton
              label={translate('TestAllLists')}
              iconName={icons.TEST}
              isSpinning={isTestingAllImportLists}
              onPress={handleTestAllImportListsPress}
            />
          </ToolbarItem>

          <ToolbarItem id="manage" priority={1} groupId="extras">
            <PageToolbarButton
              label={translate('ManageLists')}
              iconName={icons.MANAGE}
              onPress={handleManageImportListsPress}
            />
          </ToolbarItem>
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
