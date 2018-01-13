const thunks = {};

function identity(payload) {
  return payload;
}

export function createThunk(type, identityFunction = identity) {
  return function(payload = {}) {
    return function(dispatch, getState) {
      const thunk = thunks[type];

      if (thunk) {
        return thunk(getState, identityFunction(payload), dispatch);
      }

      throw Error(`Thunk handler has not been registered for ${type}`);
    };
  };
}

export function handleThunks(handlers) {
  const types = Object.keys(handlers);

  types.forEach((type) => {
    thunks[type] = handlers[type];
  });
}

