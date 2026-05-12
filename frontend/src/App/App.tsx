import { QueryClientProvider } from '@tanstack/react-query';
import React from 'react';
import DocumentTitle from 'react-document-title';
import { BrowserRouter } from 'react-router-dom';
import { Provider } from 'react-redux';
import { Store } from 'redux';
import Page from 'Components/Page/Page';
import ApplyTheme from './ApplyTheme';
import AppRoutes from './AppRoutes';
import { queryClient } from './queryClient';

interface AppProps {
  store: Store;
}

function App({ store }: AppProps) {
  return (
    <DocumentTitle title={window.Sonarr.instanceName}>
      <QueryClientProvider client={queryClient}>
        <Provider store={store}>
          <BrowserRouter basename={window.Sonarr.urlBase || undefined}>
            <ApplyTheme />
            <Page>
              <AppRoutes />
            </Page>
          </BrowserRouter>
        </Provider>
      </QueryClientProvider>
    </DocumentTitle>
  );
}

export default App;
