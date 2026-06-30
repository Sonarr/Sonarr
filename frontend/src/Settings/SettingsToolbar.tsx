import React, { ReactElement, useCallback, useEffect } from 'react';
import { useBlocker } from 'react-router';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import useKeyboardShortcuts from 'Helpers/Hooks/useKeyboardShortcuts';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import AdvancedSettingsButton from './AdvancedSettingsButton';
import PendingChangesModal from './PendingChangesModal';

interface SettingsToolbarProps {
  showSave?: boolean;
  isSaving?: boolean;
  hasPendingChanges?: boolean;
  // TODO: This should do type checking like PageToolbarSectionProps,
  // but this works for the time being.
  additionalButtons?: ReactElement | null;
  onSavePress?: () => void;
}

function SettingsToolbar({
  showSave = true,
  isSaving,
  hasPendingChanges,
  additionalButtons = null,
  onSavePress,
}: SettingsToolbarProps) {
  const { bindShortcut, unbindShortcut } = useKeyboardShortcuts();

  const blocker = useBlocker(() => !!hasPendingChanges);

  const handleConfirmNavigation = useCallback(() => {
    blocker.proceed?.();
  }, [blocker]);

  const handleCancelNavigation = useCallback(() => {
    blocker.reset?.();
  }, [blocker]);

  useEffect(() => {
    bindShortcut(
      'saveSettings',
      () => {
        if (hasPendingChanges) {
          onSavePress?.();
        }
      },
      {
        isGlobal: true,
      }
    );

    return () => {
      unbindShortcut('saveSettings');
    };
  }, [hasPendingChanges, bindShortcut, unbindShortcut, onSavePress]);

  return (
    <PageToolbar>
      <PageToolbarSection>
        <AdvancedSettingsButton showLabel={true} />
        {showSave ? (
          <PageToolbarButton
            label={
              hasPendingChanges
                ? translate('SaveChanges')
                : translate('NoChanges')
            }
            iconName={icons.SAVE}
            isSpinning={isSaving}
            isDisabled={!hasPendingChanges}
            onPress={onSavePress}
          />
        ) : null}

        {additionalButtons}
      </PageToolbarSection>

      <PendingChangesModal
        isOpen={blocker.state === 'blocked'}
        onConfirm={handleConfirmNavigation}
        onCancel={handleCancelNavigation}
      />
    </PageToolbar>
  );
}

export default SettingsToolbar;
