import { useCallback, useEffect } from 'react';
import { useBlocker } from 'react-router';
import useKeyboardShortcuts from 'Helpers/Hooks/useKeyboardShortcuts';

interface UseSettingsSaveOptions {
  hasPendingChanges?: boolean;
  onSavePress?: () => void;
}

interface PendingChangesModalProps {
  isOpen: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}

interface UseSettingsSaveResult {
  pendingChangesModalProps: PendingChangesModalProps;
}

export default function useSettingsSave({
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

  return {
    pendingChangesModalProps: {
      isOpen: blocker.state === 'blocked',
      onConfirm: handleConfirmNavigation,
      onCancel: handleCancelNavigation,
    },
  };
}
