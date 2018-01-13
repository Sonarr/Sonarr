// https://github.com/react-bootstrap/react-element-children

import React from 'react';

/**
 * Iterates through children that are typically specified as `props.children`,
 * but only maps over children that are "valid components".
 *
 * The mapFunction provided index will be normalised to the components mapped,
 * so an invalid component would not increase the index.
 *
 * @param {?*} children Children tree container.
 * @param {function(*, int)} func.
 * @param {*} context Context for func.
 * @return {object} Object containing the ordered map of results.
 */
export function map(children, func, context) {
  let index = 0;

  return React.Children.map(children, (child) => {
    if (!React.isValidElement(child)) {
      return child;
    }

    return func.call(context, child, index++);
  });
}

/**
 * Iterates through children that are "valid components".
 *
 * The provided forEachFunc(child, index) will be called for each
 * leaf child with the index reflecting the position relative to "valid components".
 *
 * @param {?*} children Children tree container.
 * @param {function(*, int)} func.
 * @param {*} context Context for context.
 */
export function forEach(children, func, context) {
  let index = 0;

  React.Children.forEach(children, (child) => {
    if (!React.isValidElement(child)) {
      return;
    }

    func.call(context, child, index++);
  });
}

/**
 * Count the number of "valid components" in the Children container.
 *
 * @param {?*} children Children tree container.
 * @returns {number}
 */
export function count(children) {
  let result = 0;

  React.Children.forEach(children, (child) => {
    if (!React.isValidElement(child)) {
      return;
    }

    ++result;
  });

  return result;
}

/**
 * Finds children that are typically specified as `props.children`,
 * but only iterates over children that are "valid components".
 *
 * The provided forEachFunc(child, index) will be called for each
 * leaf child with the index reflecting the position relative to "valid components".
 *
 * @param {?*} children Children tree container.
 * @param {function(*, int)} func.
 * @param {*} context Context for func.
 * @returns {array} of children that meet the func return statement
 */
export function filter(children, func, context) {
  const result = [];

  forEach(children, (child, index) => {
    if (func.call(context, child, index)) {
      result.push(child);
    }
  });

  return result;
}

export function find(children, func, context) {
  let result = null;

  forEach(children, (child, index) => {
    if (result) {
      return;
    }
    if (func.call(context, child, index)) {
      result = child;
    }
  });

  return result;
}

export function every(children, func, context) {
  let result = true;

  forEach(children, (child, index) => {
    if (!result) {
      return;
    }
    if (!func.call(context, child, index)) {
      result = false;
    }
  });

  return result;
}

export function some(children, func, context) {
  let result = false;

  forEach(children, (child, index) => {
    if (result) {
      return;
    }

    if (func.call(context, child, index)) {
      result = true;
    }
  });

  return result;
}

export function toArray(children) {
  const result = [];

  forEach(children, (child) => {
    result.push(child);
  });

  return result;
}
