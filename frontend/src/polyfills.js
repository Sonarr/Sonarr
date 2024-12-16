/* eslint no-empty-function: 0, no-extend-native: 0, "@typescript-eslint/no-empty-function": 0 */

window.console = window.console || {};
window.console.log = window.console.log || function() {};
window.console.group = window.console.group || function() {};
window.console.groupEnd = window.console.groupEnd || function() {};
window.console.debug = window.console.debug || function() {};
window.console.warn = window.console.warn || function() {};
window.console.assert = window.console.assert || function() {};

// TODO: Remove in v5, well suppoprted in browsers
if (!String.prototype.startsWith) {
  Object.defineProperty(String.prototype, 'startsWith', {
    enumerable: false,
    configurable: false,
    writable: false,
    value(searchString, position) {
      position = position || 0;
      return this.indexOf(searchString, position) === position;
    }
  });
}

// TODO: Remove in v5, well suppoprted in browsers
if (!String.prototype.endsWith) {
  Object.defineProperty(String.prototype, 'endsWith', {
    enumerable: false,
    configurable: false,
    writable: false,
    value(searchString, position) {
      position = position || this.length;
      position = position - searchString.length;
      const lastIndex = this.lastIndexOf(searchString);
      return lastIndex !== -1 && lastIndex === position;
    }
  });
}

// TODO: Remove in v5, use `includes` instead
if (!('contains' in String.prototype)) {
  String.prototype.contains = function(str, startIndex) {
    return String.prototype.indexOf.call(this, str, startIndex) !== -1;
  };
}

// For Firefox ESR 115 support
if (!Object.groupBy) {
  import('core-js/actual/object/group-by');
}
