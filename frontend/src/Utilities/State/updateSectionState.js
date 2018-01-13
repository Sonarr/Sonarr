function updateSectionState(state, section, newState) {
  const [, subSection] = section.split('.');

  if (subSection) {
    return Object.assign({}, state, { [subSection]: newState });
  }

  // TODO: Remove in favour of using subSection
  if (state.hasOwnProperty(section)) {
    return Object.assign({}, state, { [section]: newState });
  }

  return Object.assign({}, state, newState);
}

export default updateSectionState;
