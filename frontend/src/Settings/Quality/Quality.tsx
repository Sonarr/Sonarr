import React, { useCallback, useRef, useState } from 'react';
import { useSelector } from 'react-redux';
import * as commandNames from 'Commands/commandNames';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import { icons } from 'Helpers/Props';
import SettingsToolbar from 'Settings/SettingsToolbar';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import {
  SaveCallback,
  SettingsStateChange,
} from 'typings/Settings/SettingsState';
import translate from 'Utilities/String/translate';
import QualityDefinitions from './Definition/QualityDefinitions';
import ResetQualityDefinitionsModal from './Reset/ResetQualityDefinitionsModal';

function Quality() {
  const isResettingQualityDefinitions = useSelector(
    createCommandExecutingSelector(commandNames.RESET_QUALITY_DEFINITIONS)
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

      <ResetQualityDefinitionsModal
        isOpen={isConfirmQualityDefinitionResetModalOpen}
        onModalClose={handleCloseResetQualityDefinitionsModal}
      />
    </PageContent>
  );
}

export default Quality;
