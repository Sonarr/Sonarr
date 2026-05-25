import { useCallback, useEffect } from 'react';
import { useBlocker } from 'react-router';
import { type MoreMenuItem } from 'Components/Page/Toolbar/PageToolbar';
import useKeyboardShortcuts from 'Helpers/Hooks/useKeyboardShortcuts';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

interface UseSettingsSaveOptions {
  showSave?: boolean;
  isSaving?: boolean;
  hasPendingChanges?: boolean;
  onSavePress?: () => void;
}

interface PendingChangesModalProps {
  isOpen: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}

interface UseSettingsSaveResult {
  saveMoreMenuItem: MoreMenuItem | null;
  pendingChangesModalProps: PendingChangesModalProps;
}

export default function useSettingsSave({
  showSave = true,
  isSaving = false,
  hasPendingChanges = false,
  onSavePress,
}: UseSettingsSaveOptions): UseSettingsSaveResult {
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
      { isGlobal: true }
    );

    return () => {
      unbindShortcut('saveSettings');
    };
  }, [hasPendingChanges, bindShortcut, unbindShortcut, onSavePress]);

  const saveMoreMenuItem: MoreMenuItem | null = showSave
    ? {
        id: 'save',
        label: hasPendingChanges
          ? translate('SaveChanges')
          : translate('NoChanges'),
        iconName: icons.SAVE,
        isSpinning: isSaving,
        isDisabled: !hasPendingChanges,
        onPress: onSavePress,
      }
    : null;

  return {
    saveMoreMenuItem,
    pendingChangesModalProps: {
      isOpen: blocker.state === 'blocked',
      onConfirm: handleConfirmNavigation,
      onCancel: handleCancelNavigation,
    },
  };
}
