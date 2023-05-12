import { Dispatch } from 'redux';
import AppState from 'App/State/AppState';

type GetState = () => AppState;
type Thunk = (
  getState: GetState,
  identity: unknown,
  dispatch: Dispatch
) => unknown;

const thunks: Record<string, Thunk> = {};

function identity(payload: unknown) {
  return payload;
}

export function createThunk(type: string, identityFunction = identity) {
  return function (payload: unknown = {}) {
    return function (dispatch: Dispatch, getState: GetState) {
      const thunk = thunks[type];

      if (thunk) {
        return thunk(getState, identityFunction(payload), dispatch);
      }

      throw Error(`Thunk handler has not been registered for ${type}`);
    };
  };
}

export function handleThunks(handlers: Record<string, Thunk>) {
  const types = Object.keys(handlers);

  types.forEach((type) => {
    thunks[type] = handlers[type];
  });
}
