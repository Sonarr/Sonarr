import _ from 'lodash';

function getSectionState(state, section, isFullStateTree = false) {
  if (isFullStateTree) {
    return _.get(state, section);
  }

  const [, subSection] = section.split('.');

  if (subSection) {
    return Object.assign({}, state[subSection]);
  }

  // TODO: Remove in favour of using subSection
  if (state.hasOwnProperty(section)) {
    return Object.assign({}, state[section]);
  }

  return Object.assign({}, state);
}

export default getSectionState;
