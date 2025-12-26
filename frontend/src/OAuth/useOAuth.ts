import { useCallback, useState } from 'react';
import { type Error } from 'App/State/AppSectionState';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import requestAction from 'Utilities/requestAction';

const callbackUrl = `${window.location.origin}${window.Sonarr.urlBase}/oauth.html`;

interface OAuthResult {
  [key: string]: string | number | boolean;
}

interface OAuthState {
  authorizing: boolean;
  result: OAuthResult | null;
  error: Error | null;
}

interface StartOAuthParams {
  name: string;
  provider?: string;
  providerData?: Record<string, unknown>;
  [key: string]: unknown;
}

interface OAuthResponse {
  oauthUrl?: string;
  poll?: boolean;
  success?: boolean;
  [key: string]: unknown;
}

interface QueryParams {
  [key: string]: string;
}

interface WindowWithOAuth extends Window {
  onCompleteOauth?: (query: string, onComplete: () => void) => void;
}

function showOAuthWindow(
  url: string,
  payload: StartOAuthParams,
  poll = false,
  ajaxOptions?: Record<string, unknown>
): Promise<QueryParams> {
  return new Promise((resolve, reject) => {
    const selfWindow = window as WindowWithOAuth;
    const newWindow = window.open(url);

    if (
      !newWindow ||
      newWindow.closed ||
      typeof newWindow.closed === 'undefined'
    ) {
      // A fake validation error to mimic a 400 response from the API.
      const error = Object.assign(
        new Error('Pop-ups are being blocked by your browser'),
        {
          status: 400,
          responseJSON: [
            {
              propertyName: payload.name,
              errorMessage: 'Pop-ups are being blocked by your browser',
            },
          ],
        }
      );

      return reject(error);
    }

    if (poll) {
      const pollAction = () => {
        requestAction({
          action: 'pollOAuth',
          queryParams: ajaxOptions,
          ...payload,
        }).then((response: OAuthResponse) => {
          if (response.success) {
            resolve({});
          } else {
            setTimeout(() => {
              pollAction();
            }, 5000);
          }
        });
      };

      setTimeout(() => {
        pollAction();
      }, 5000);
    } else {
      selfWindow.onCompleteOauth = function (
        query: string,
        onComplete: () => void
      ) {
        delete selfWindow.onCompleteOauth;

        const queryParams: Record<string, string> = {};
        const splitQuery = query.substring(1).split('&');

        splitQuery.forEach((param) => {
          if (param) {
            const paramSplit = param.split('=');
            queryParams[paramSplit[0]] = paramSplit[1];
          }
        });

        onComplete();
        resolve(queryParams);
      };
    }
  });
}

function executeIntermediateRequest(
  payload: Record<string, unknown>,
  ajaxOptions: Record<string, unknown>
): Promise<OAuthResponse> {
  return createAjaxRequest(ajaxOptions).request.then(
    (data: Record<string, unknown>) => {
      return requestAction({
        action: 'continueOAuth',
        queryParams: {
          ...data,
          callbackUrl,
        },
        ...payload,
      });
    }
  );
}

const useOAuth = () => {
  const [oAuthState, setOAuthState] = useState<OAuthState>({
    authorizing: false,
    result: null,
    error: null,
  });

  const setOAuthValue = useCallback((values: Partial<OAuthState>) => {
    setOAuthState((prev) => ({ ...prev, ...values }));
  }, []);

  const resetOAuth = useCallback(() => {
    setOAuthState({
      authorizing: false,
      result: null,
      error: null,
    });
  }, []);

  const startOAuth = useCallback(
    async (params: StartOAuthParams) => {
      const { name, ...otherPayload } = params;

      const actionPayload = {
        action: 'startOAuth',
        queryParams: { callbackUrl },
        ...otherPayload,
      };

      setOAuthValue({ authorizing: true });

      try {
        let startResponse: OAuthResponse = {};

        const response = (await requestAction(actionPayload)) as OAuthResponse;
        startResponse = response;

        let queryParams: QueryParams | null = null;

        if (response.oauthUrl) {
          queryParams = await showOAuthWindow(response.oauthUrl, params);
        } else {
          const intermediateResponse = await executeIntermediateRequest(
            otherPayload,
            response // Pass the entire response as ajaxOptions
          );
          startResponse = intermediateResponse;

          if (!intermediateResponse.oauthUrl) {
            throw new Error('No OAuth URL received from intermediate request');
          }

          queryParams = await showOAuthWindow(
            intermediateResponse.oauthUrl,
            params,
            intermediateResponse.poll || false,
            intermediateResponse
          );
        }

        const tokenResponse = await requestAction({
          action: 'getOAuthToken',
          queryParams: {
            ...startResponse,
            ...queryParams,
          },
          ...otherPayload,
        });

        setOAuthValue({
          authorizing: false,
          result: tokenResponse,
          error: null,
        });

        return tokenResponse;
      } catch (error) {
        const oAuthError = error as Error;
        setOAuthValue({
          authorizing: false,
          result: null,
          error: oAuthError,
        });

        throw error;
      }
    },
    [setOAuthValue]
  );

  return {
    ...oAuthState,
    startOAuth,
    setOAuthValue,
    resetOAuth,
  };
};

export default useOAuth;
