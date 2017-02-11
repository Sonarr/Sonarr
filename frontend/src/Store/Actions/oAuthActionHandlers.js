/* eslint callback-return: 0 */
import _ from 'lodash';
import $ from 'jquery';
import requestAction from 'Utilities/requestAction';
import * as types from './actionTypes';
import { setOAuthValue } from './oAuthActions';

function showOAuthWindow(url) {
  const deferred = $.Deferred();
  const selfWindow = window;

  window.open(url);

  selfWindow.onCompleteOauth = function(query, callback) {
    delete selfWindow.onCompleteOauth;

    const queryParams = {};
    const splitQuery = query.substring(1).split('&');

    _.each(splitQuery, (param) => {
      const paramSplit = param.split('=');

      queryParams[paramSplit[0]] = paramSplit[1];
    });

    callback();
    deferred.resolve(queryParams);
  };

  return deferred.promise();
}

const oAuthActionHandlers = {

  [types.START_OAUTH]: function(payload) {
    return (dispatch, getState) => {
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
    };
  }

};

export default oAuthActionHandlers;
