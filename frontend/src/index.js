import { createBrowserHistory } from 'history';
import React from 'react';
import { render } from 'react-dom';
import { fetchTranslations } from 'Utilities/String/translate';

import './preload';
import './polyfills';
import 'Diag/ConsoleApi';
import 'Styles/globals.css';
import './index.css';

const history = createBrowserHistory();
const hasTranslationsError = !await fetchTranslations();

const { default: createAppStore } = await import('Store/createAppStore');
const { default: App } = await import('./App/App');

const store = createAppStore(history);

render(
  <App
    store={store}
    history={history}
    hasTranslationsError={hasTranslationsError}
  />,
  document.getElementById('root')
);
