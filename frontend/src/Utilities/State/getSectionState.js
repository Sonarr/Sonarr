function getSectionState(state, section) {
  if (state.hasOwnProperty(section)) {
    return Object.assign({}, state[section]);
  }

  return Object.assign({}, state);
}

export default getSectionState;
