import Mousetrap, { MousetrapInstance } from 'mousetrap';
import React, { Component, ComponentType } from 'react';
import translate from 'Utilities/String/translate';

export interface Shortcut {
  key: string;
  name: string;
}

interface BindingOptions {
  isGlobal?: boolean;
}

interface KeyboardShortcutsProps {
  bindShortcut: (
    key: string,
    callback: (e: Mousetrap.ExtendedKeyboardEvent, combo: string) => void,
    options?: BindingOptions
  ) => void;
  unbindShortcut: (key: string) => void;
}

export const shortcuts: Record<string, Shortcut> = {
  OPEN_KEYBOARD_SHORTCUTS_MODAL: {
    key: '?',
    get name() {
      return translate('KeyboardShortcutsOpenModal');
    },
  },

  CLOSE_MODAL: {
    key: 'Esc',
    get name() {
      return translate('KeyboardShortcutsCloseModal');
    },
  },

  SERIES_SEARCH_INPUT: {
    key: 's',
    get name() {
      return translate('KeyboardShortcutsFocusSearchBox');
    },
  },

  SAVE_SETTINGS: {
    key: 'mod+s',
    get name() {
      return translate('KeyboardShortcutsSaveSettings');
    },
  },
};

function keyboardShortcuts(
  WrappedComponent: ComponentType<KeyboardShortcutsProps>
) {
  class KeyboardShortcuts extends Component {
    //
    // Lifecycle

    constructor(props: never) {
      super(props);
      this._mousetrapBindings = {};
      this._mousetrap = new Mousetrap();
      this._mousetrap.stopCallback = this.stopCallback;
    }

    componentWillUnmount() {
      this.unbindAllShortcuts();
      this._mousetrap = null;
    }

    _mousetrap: MousetrapInstance | null;
    _mousetrapBindings: Record<string, BindingOptions>;

    //
    // Control

    bindShortcut = (
      key: string,
      callback: (e: Mousetrap.ExtendedKeyboardEvent, combo: string) => void,
      options: BindingOptions = {}
    ) => {
      this._mousetrap?.bind(key, callback);
      this._mousetrapBindings[key] = options;
    };

    unbindShortcut = (key: string) => {
      if (this._mousetrap != null) {
        delete this._mousetrapBindings[key];
        this._mousetrap.unbind(key);
      }
    };

    unbindAllShortcuts = () => {
      const keys = Object.keys(this._mousetrapBindings);

      if (!keys.length) {
        return;
      }

      keys.forEach((binding) => {
        this._mousetrap?.unbind(binding);
      });

      this._mousetrapBindings = {};
    };

    stopCallback = (
      _e: Mousetrap.ExtendedKeyboardEvent,
      element: Element,
      combo: string
    ) => {
      const binding = this._mousetrapBindings[combo];

      if (!binding || binding.isGlobal) {
        return false;
      }

      return (
        element.tagName === 'INPUT' ||
        element.tagName === 'SELECT' ||
        element.tagName === 'TEXTAREA' ||
        ('contentEditable' in element && element.contentEditable === 'true')
      );
    };

    //
    // Render

    render() {
      return (
        <WrappedComponent
          {...this.props}
          bindShortcut={this.bindShortcut}
          unbindShortcut={this.unbindShortcut}
        />
      );
    }
  }

  return KeyboardShortcuts;
}

export default keyboardShortcuts;
