import $ from 'jquery';
import { createAction } from 'redux-actions';
import requestAction from 'Utilities/requestAction';
import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';
import { createThunk, handleThunks } from 'Store/thunks';
import createHandleActions from './Creators/createHandleActions';

//
// Variables

export const section = 'oAuth';

//
// State

export const defaultState = {
  authorizing: false,
  result: null,
  error: null
};

//
// Actions Types

export const START_OAUTH = 'oAuth/startOAuth';
export const SET_OAUTH_VALUE = 'oAuth/setOAuthValue';
export const RESET_OAUTH = 'oAuth/resetOAuth';

//
// Action Creators

export const startOAuth = createThunk(START_OAUTH);
export const setOAuthValue = createAction(SET_OAUTH_VALUE);
export const resetOAuth = createAction(RESET_OAUTH);

//
// Helpers

function showOAuthWindow(url) {
  const deferred = $.Deferred();
  const selfWindow = window;

  window.open(url);

  selfWindow.onCompleteOauth = function(query, onComplete) {
    delete selfWindow.onCompleteOauth;

    const queryParams = {};
    const splitQuery = query.substring(1).split('&');

    splitQuery.forEach((param) => {
      if (param) {
        const paramSplit = param.split('=');

        queryParams[paramSplit[0]] = paramSplit[1];
      }
    });

    onComplete();
    deferred.resolve(queryParams);
  };

  return deferred.promise();
}

//
// Action Handlers

export const actionHandlers = handleThunks({

  [START_OAUTH]: function(getState, payload, dispatch) {
    const actionPayload = {
      action: 'startOAuth',
      queryParams: { callbackUrl: `${window.location.origin}${window.Sonarr.urlBase}/oauth.html` },
      ...payload
    };

    dispatch(setOAuthValue({
      authorizing: true
    }));

    let startResponse = {};

    const promise = requestAction(actionPayload)
      .then((response) => {
        startResponse = response;
        return showOAuthWindow(response.oauthUrl);
      })
      .then((queryParams) => {
        return requestAction({
          action: 'getOAuthToken',
          queryParams: {
            ...startResponse,
            ...queryParams
          },
          ...payload
        });
      })
      .then((response) => {
        dispatch(setOAuthValue({
          authorizing: false,
          result: response,
          error: null
        }));
      });

    promise.fail((xhr) => {
      dispatch(setOAuthValue({
        authorizing: false,
        result: null,
        error: xhr
      }));
    });
  }

});

//
// Reducers

export const reducers = createHandleActions({

  [SET_OAUTH_VALUE]: function(state, { payload }) {
    const newState = Object.assign(getSectionState(state, section), payload);

    return updateSectionState(state, section, newState);
  },

  [RESET_OAUTH]: function(state) {
    return updateSectionState(state, section, defaultState);
  }

}, defaultState, section);
