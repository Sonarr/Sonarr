import Mousetrap from 'mousetrap';
import React, { Component } from 'react';
import getDisplayName from 'Helpers/getDisplayName';

export const shortcuts = {
  OPEN_KEYBOARD_SHORTCUTS_MODAL: {
    key: '?',
    name: 'Open This Modal'
  },

  CLOSE_MODAL: {
    key: 'Esc',
    name: 'Close Current Modal'
  },

  ACCEPT_CONFIRM_MODAL: {
    key: 'Enter',
    name: 'Accept Confirmation Modal'
  },

  SERIES_SEARCH_INPUT: {
    key: 's',
    name: 'Focus Search Box'
  },

  SAVE_SETTINGS: {
    key: 'mod+s',
    name: 'Save Settings'
  }
};

function keyboardShortcuts(WrappedComponent) {
  class KeyboardShortcuts extends Component {

    //
    // Lifecycle

    constructor(props, context) {
      super(props, context);
      this._mousetrapBindings = {};
      this._mousetrap = new Mousetrap();
      this._mousetrap.stopCallback = this.stopCallback;
    }

    componentWillUnmount() {
      this.unbindAllShortcuts();
      this._mousetrap = null;
    }

    //
    // Control

    bindShortcut = (key, callback, options = {}) => {
      this._mousetrap.bind(key, callback);
      this._mousetrapBindings[key] = options;
    };

    unbindShortcut = (key) => {
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
        this._mousetrap.unbind(binding);
      });

      this._mousetrapBindings = {};
    };

    stopCallback = (event, element, combo) => {
      const binding = this._mousetrapBindings[combo];

      if (!binding || binding.isGlobal) {
        return false;
      }

      return (
        element.tagName === 'INPUT' ||
        element.tagName === 'SELECT' ||
        element.tagName === 'TEXTAREA' ||
        (element.contentEditable && element.contentEditable === 'true')
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

  KeyboardShortcuts.displayName = `KeyboardShortcut(${getDisplayName(WrappedComponent)})`;
  KeyboardShortcuts.WrappedComponent = WrappedComponent;

  return KeyboardShortcuts;
}

export default keyboardShortcuts;
