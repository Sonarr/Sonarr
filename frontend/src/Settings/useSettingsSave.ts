import { useEffect } from 'react';
import useKeyboardShortcuts from 'Helpers/Hooks/useKeyboardShortcuts';
import useUnsavedChanges from 'Helpers/Hooks/useUnsavedChanges';

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
  const { pendingChangesModalProps } = useUnsavedChanges({
    hasUnsavedChanges: hasPendingChanges,
  });

  useEffect(() => {
    bindShortcut(
      'saveSettings',
      (e) => {
        e.preventDefault();

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
    pendingChangesModalProps,
  };
}
