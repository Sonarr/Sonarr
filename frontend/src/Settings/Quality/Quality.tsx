import React, { useCallback, useRef, useState } from 'react';
import CommandNames from 'Commands/CommandNames';
import { useCommandExecuting, useExecuteCommand } from 'Commands/useCommands';
import ConfirmModal from 'Components/Modal/ConfirmModal';
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
import QualityDefinitions from './Definition/QualityDefinitions';

function Quality() {
  const executeCommand = useExecuteCommand();
  const isResettingQualityDefinitions = useCommandExecuting(
    CommandNames.ResetQualityDefinitions
  );

  const saveDefinitions = useRef<() => void>();

  const [isSaving, setIsSaving] = useState(false);

  const [
    isConfirmQualityDefinitionResetModalOpen,
    setIsConfirmQualityDefinitionResetModalOpen,
  ] = useState(false);

  const [hasPendingChanges, setHasPendingChanges] = useState(false);

  const handleSetChildSave = useCallback((saveCallback: SaveCallback) => {
    saveDefinitions.current = saveCallback;
  }, []);

  const handleChildStateChange = useCallback(
    ({ isSaving, hasPendingChanges }: SettingsStateChange) => {
      setIsSaving(isSaving);
      setHasPendingChanges(hasPendingChanges);
    },
    []
  );

  const handleResetQualityDefinitionsPress = useCallback(() => {
    setIsConfirmQualityDefinitionResetModalOpen(true);
  }, []);

  const handleCloseResetQualityDefinitionsModal = useCallback(() => {
    setIsConfirmQualityDefinitionResetModalOpen(false);
  }, []);

  const handleResetQualityDefinitionsConfirmed = useCallback(() => {
    executeCommand({
      name: CommandNames.ResetQualityDefinitions,
      resetTitles: true,
    });

    setIsConfirmQualityDefinitionResetModalOpen(false);
  }, [executeCommand]);

  const handleSavePress = useCallback(() => {
    saveDefinitions.current?.();
  }, []);

  return (
    <PageContent title={translate('QualitySettings')}>
      <SettingsToolbar
        isSaving={isSaving}
        hasPendingChanges={hasPendingChanges}
        additionalButtons={
          <>
            <PageToolbarSeparator />

            <PageToolbarButton
              label={translate('ResetDefinitions')}
              iconName={icons.REFRESH}
              isSpinning={isResettingQualityDefinitions}
              isDisabled={isResettingQualityDefinitions}
              onPress={handleResetQualityDefinitionsPress}
            />
          </>
        }
        onSavePress={handleSavePress}
      />
      <PageContentBody>
        <QualityDefinitions
          isResettingQualityDefinitions={isResettingQualityDefinitions}
          setChildSave={handleSetChildSave}
          onChildStateChange={handleChildStateChange}
        />
      </PageContentBody>

      <ConfirmModal
        isOpen={isConfirmQualityDefinitionResetModalOpen}
        kind="danger"
        title={translate('ResetQualityDefinitions')}
        message={translate('ResetQualityDefinitionsMessageText')}
        confirmLabel={translate('Reset')}
        onConfirm={handleResetQualityDefinitionsConfirmed}
        onCancel={handleCloseResetQualityDefinitionsModal}
      />
    </PageContent>
  );
}

export default Quality;
