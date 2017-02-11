function createReducers(sections, createReducer) {
  const reducers = {};

  sections.forEach((section) => {
    reducers[section] = createReducer(section);
  });

  return (state, action) => {
    const section = action.payload.section;
    const reducer = reducers[section];

    if (reducer) {
      return reducer(state, action);
    }

    return state;
  };
}

export default createReducers;
