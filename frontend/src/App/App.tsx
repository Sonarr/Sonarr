import { QueryClientProvider } from '@tanstack/react-query';
import React from 'react';
import DocumentTitle from 'react-document-title';
import {
  createBrowserRouter,
  createRoutesFromElements,
  Outlet,
  Route,
  RouterProvider,
} from 'react-router-dom';
import { Provider } from 'react-redux';
import { Store } from 'redux';
import Page from 'Components/Page/Page';
import ApplyTheme from './ApplyTheme';
import { appRouteElements } from './AppRoutes';
import { queryClient } from './queryClient';

interface AppProps {
  store: Store;
}

function PageLayout() {
  return (
    <>
      <ApplyTheme />
      <Page>
        <Outlet />
      </Page>
    </>
  );
}

const router = createBrowserRouter(
  createRoutesFromElements(
    <Route element={<PageLayout />}>{appRouteElements()}</Route>
  ),
  { basename: window.Sonarr.urlBase || undefined }
);

function App({ store }: AppProps) {
  return (
    <DocumentTitle title={window.Sonarr.instanceName}>
      <QueryClientProvider client={queryClient}>
        <Provider store={store}>
          <RouterProvider router={router} />
        </Provider>
      </QueryClientProvider>
    </DocumentTitle>
  );
}

export default App;
