import { useCallback, useState } from 'react';
import { useBlocker } from 'react-router';
import useBeforeUnload from './useBeforeUnload';

interface UseUnsavedChangesOptions {
  hasUnsavedChanges: boolean;
  onDiscard?: () => void;
}

function useUnsavedChanges({
  hasUnsavedChanges,
  onDiscard,
}: UseUnsavedChangesOptions) {
  const blocker = useBlocker(hasUnsavedChanges);
  const [isClosePending, setIsClosePending] = useState(false);
  useBeforeUnload(hasUnsavedChanges);

  const handleClose = useCallback(() => {
    if (hasUnsavedChanges) {
      setIsClosePending(true);
    } else {
      onDiscard?.();
    }
  }, [hasUnsavedChanges, onDiscard]);

  const handleConfirm = useCallback(() => {
    if (blocker.state === 'blocked') {
      blocker.proceed();
      onDiscard?.();
    } else if (isClosePending) {
      onDiscard?.();
    }

    setIsClosePending(false);
  }, [blocker, isClosePending, onDiscard]);

  const handleCancel = useCallback(() => {
    blocker.reset?.();
    setIsClosePending(false);
  }, [blocker]);

  return {
    onClose: handleClose,
    pendingChangesModalProps: {
      isOpen: blocker.state === 'blocked' || isClosePending,
      isClosing: isClosePending && blocker.state !== 'blocked',
      onConfirm: handleConfirm,
      onCancel: handleCancel,
    },
  };
}

export default useUnsavedChanges;
