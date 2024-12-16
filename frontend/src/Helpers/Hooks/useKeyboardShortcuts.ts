import Mousetrap, { MousetrapInstance } from 'mousetrap';
import { useCallback, useEffect, useMemo, useRef } from 'react';
import translate from 'Utilities/String/translate';

export interface Shortcut {
  key: string;
  name: string;
}

interface BindingOptions {
  isGlobal?: boolean;
}

export const shortcuts = {
  openKeyboardShortcutsModal: {
    key: '?',
    get name() {
      return translate('KeyboardShortcutsOpenModal');
    },
  },

  closeModal: {
    key: 'Esc',
    get name() {
      return translate('KeyboardShortcutsCloseModal');
    },
  },

  acceptConfirmModal: {
    key: 'Enter',
    get name() {
      return translate('KeyboardShortcutsConfirmModal');
    },
  },

  focusSeriesSearchInput: {
    key: 's',
    get name() {
      return translate('KeyboardShortcutsFocusSearchBox');
    },
  },

  saveSettings: {
    key: 'mod+s',
    get name() {
      return translate('KeyboardShortcutsSaveSettings');
    },
  },
};

function useKeyboardShortcuts() {
  const bindings = useRef<Record<string, BindingOptions>>({});
  const mouseTrap = useRef<MousetrapInstance | null>();

  const handleStop = useCallback(
    (_e: Mousetrap.ExtendedKeyboardEvent, element: Element, combo: string) => {
      const binding = bindings.current[combo];

      if (!binding || binding.isGlobal) {
        return false;
      }

      return (
        element.tagName === 'INPUT' ||
        element.tagName === 'SELECT' ||
        element.tagName === 'TEXTAREA' ||
        ('contentEditable' in element && element.contentEditable === 'true')
      );
    },
    []
  );

  const bindShortcut = useCallback(
    (
      shortcutKey: keyof typeof shortcuts,
      callback: (e: Mousetrap.ExtendedKeyboardEvent, combo: string) => void,
      options: BindingOptions = {}
    ) => {
      const shortcut = shortcuts[shortcutKey];

      mouseTrap.current?.bind(shortcut.key, callback);
      bindings.current[shortcut.key] = options;
    },
    []
  );

  const unbindShortcut = useCallback((shortcutKey: keyof typeof shortcuts) => {
    const shortcut = shortcuts[shortcutKey];

    delete bindings.current[shortcut.key];
    mouseTrap.current?.unbind(shortcut.key);
  }, []);

  useEffect(() => {
    mouseTrap.current = new Mousetrap();
    mouseTrap.current.stopCallback = handleStop;

    const localMouseTrap = mouseTrap.current;

    return () => {
      const keys = Object.keys(bindings.current);

      if (!keys.length) {
        return;
      }

      keys.forEach((binding) => {
        localMouseTrap.unbind(binding);
      });

      bindings.current = {};
      mouseTrap.current = null;
    };
  }, [handleStop]);

  return useMemo(
    () => ({ bindShortcut, unbindShortcut }),
    [bindShortcut, unbindShortcut]
  );
}

export default useKeyboardShortcuts;
