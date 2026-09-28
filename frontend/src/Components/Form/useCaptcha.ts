import { useCallback, useState } from 'react';
import requestAction from 'Utilities/requestAction';

interface CaptchaState {
  refreshing: boolean;
  token: string | null;
  siteKey: string | null;
  secretToken: string | null;
  ray: string | null;
  stoken: string | null;
  responseUrl: string | null;
}

interface CaptchaRequest {
  siteKey?: string | null;
  secretToken?: string | null;
  ray?: string | null;
  stoken?: string | null;
  responseUrl?: string | null;
}

interface ProviderParams {
  provider: string;
  providerData: object;
}

const defaultState: CaptchaState = {
  refreshing: false,
  token: null,
  siteKey: null,
  secretToken: null,
  ray: null,
  stoken: null,
  responseUrl: null,
};

function useCaptcha() {
  const [state, setState] = useState<CaptchaState>(defaultState);

  const refresh = useCallback(({ provider, providerData }: ProviderParams) => {
    setState((prevState) => ({ ...prevState, refreshing: true }));

    const promise = requestAction({
      action: 'checkCaptcha',
      provider,
      providerData,
    });

    promise.done((data: { captchaRequest?: CaptchaRequest }) => {
      setState((prevState) => ({
        ...prevState,
        refreshing: false,
        ...data.captchaRequest,
      }));
    });

    promise.fail(() => {
      setState((prevState) => ({ ...prevState, refreshing: false }));
    });
  }, []);

  const getCaptchaCookie = useCallback(
    ({
      provider,
      providerData,
      captchaResponse,
    }: ProviderParams & { captchaResponse: string }) => {
      const promise = requestAction({
        action: 'getCaptchaCookie',
        provider,
        providerData,
        queryParams: {
          responseUrl: state.responseUrl,
          ray: state.ray,
          captchaResponse,
        },
      });

      promise.done((data: { captchaToken: string }) => {
        setState((prevState) => ({ ...prevState, token: data.captchaToken }));
      });
    },
    [state.responseUrl, state.ray]
  );

  const reset = useCallback(() => {
    setState(defaultState);
  }, []);

  return {
    ...state,
    refresh,
    getCaptchaCookie,
    reset,
  };
}

export default useCaptcha;
