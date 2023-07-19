import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import ParseAppState from 'App/State/ParseAppState';

export default function parseStateSelector() {
  return createSelector(
    (state: AppState) => state.parse,
    (parse: ParseAppState) => {
      return parse;
    }
  );
}
