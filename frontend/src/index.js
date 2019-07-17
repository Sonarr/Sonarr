import React from 'react';
import { render } from 'react-dom';
import { createBrowserHistory } from 'history';
import createAppStore from 'Store/createAppStore';
import App from './App/App';
import 'Styles/globals.css';
import './index.css';

const history = createBrowserHistory();
const store = createAppStore(history);

render(
  <App
    store={store}
    history={history}
  />,
  document.getElementById('root')
);
