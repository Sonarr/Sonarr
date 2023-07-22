import { createBrowserHistory } from 'history';
import React from 'react';
import { render } from 'react-dom';
import createAppStore from 'Store/createAppStore';
import { fetchTranslations } from 'Utilities/String/translate';
import App from './App/App';

import 'Diag/ConsoleApi';

export async function bootstrap() {
  const history = createBrowserHistory();
  const store = createAppStore(history);
  const hasTranslationsError = !(await fetchTranslations());

  render(
    <App
      store={store}
      history={history}
      hasTranslationsError={hasTranslationsError}
    />,
    document.getElementById('root')
  );
}
