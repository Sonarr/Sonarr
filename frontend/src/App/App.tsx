import { ConnectedRouter, ConnectedRouterProps } from 'connected-react-router';
import React from 'react';
import DocumentTitle from 'react-document-title';
import { Provider } from 'react-redux';
import { Store } from 'redux';
import PageConnector from 'Components/Page/PageConnector';
import ApplyTheme from './ApplyTheme';
import AppRoutes from './AppRoutes';

interface AppProps {
  store: Store;
  history: ConnectedRouterProps['history'];
}

function App({ store, history }: AppProps) {
  return (
    <DocumentTitle title={window.Sonarr.instanceName}>
      <Provider store={store}>
        <ConnectedRouter history={history}>
          <ApplyTheme />
          <PageConnector>
            <AppRoutes />
          </PageConnector>
        </ConnectedRouter>
      </Provider>
    </DocumentTitle>
  );
}

export default App;
