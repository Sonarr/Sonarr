import { Dispatch } from 'redux';
import AppState from 'App/State/AppState';

type GetState = () => AppState;
type Thunk = (
  getState: GetState,
  identityFn: never,
  dispatch: Dispatch
) => unknown;

const thunks: Record<string, Thunk> = {};

function identity<T, TResult>(payload: T): TResult {
  return payload as unknown as TResult;
}

export function createThunk(type: string, identityFunction = identity) {
  return function <T>(payload?: T) {
    return function (dispatch: Dispatch, getState: GetState) {
      const thunk = thunks[type];

      if (thunk) {
        const finalPayload = payload ?? {};

        return thunk(getState, identityFunction(finalPayload), dispatch);
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
