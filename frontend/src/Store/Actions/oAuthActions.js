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
  accessToken: null,
  accessTokenSecret: null
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
      const paramSplit = param.split('=');

      queryParams[paramSplit[0]] = paramSplit[1];
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
      queryParams: { callbackUrl: `${window.location.origin}/oauth.html` },
      ...payload
    };

    dispatch(setOAuthValue({
      authorizing: true
    }));

    const promise = requestAction(actionPayload)
      .then((response) => {
        return showOAuthWindow(response.oauthUrl);
      })
      .then((queryParams) => {
        return requestAction({
          action: 'getOAuthToken',
          queryParams,
          ...payload
        });
      })
      .then((response) => {
        const {
          accessToken,
          accessTokenSecret
        } = response;

        dispatch(setOAuthValue({
          authorizing: false,
          accessToken,
          accessTokenSecret
        }));
      });

    promise.fail(() => {
      dispatch(setOAuthValue({
        authorizing: false
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
