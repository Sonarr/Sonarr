import { QueryClientProvider } from '@tanstack/react-query';
import React from 'react';
import {
  createBrowserRouter,
  createRoutesFromElements,
  Outlet,
  Route,
  RouterProvider,
} from 'react-router-dom';
import Page from 'Components/Page/Page';
import ApplyTheme from './ApplyTheme';
import { appRouteElements } from './AppRoutes';
import { queryClient } from './queryClient';

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

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <RouterProvider router={router} />
    </QueryClientProvider>
  );
}

export default App;
