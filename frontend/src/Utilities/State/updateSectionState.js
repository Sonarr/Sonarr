function updateSectionState(state, section, newState) {
  if (state.hasOwnProperty(section)) {
    return Object.assign({}, state, { [section]: newState });
  }

  return Object.assign({}, state, newState);
}

export default updateSectionState;
