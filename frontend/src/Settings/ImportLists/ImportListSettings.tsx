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
    <PageContent title={translate('ImportListSettings')}>
      <SettingsToolbar
        isSaving={isSaving}
        hasPendingChanges={hasPendingChanges}
        additionalButtons={
          <>
            <PageToolbarSeparator />

            <PageToolbarButton
              label={translate('TestAllLists')}
              iconName={icons.TEST}
              isSpinning={isTestingAllImportLists}
              onPress={handleTestAllImportListsPress}
            />

            <PageToolbarButton
              label={translate('ManageLists')}
              iconName={icons.MANAGE}
              onPress={handleManageImportListsPress}
            />
          </>
        }
        onSavePress={handleSavePress}
      />

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
    </PageContent>
  );
}

export default ImportListSettings;
