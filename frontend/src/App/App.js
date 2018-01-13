import PropTypes from 'prop-types';
import React from 'react';
import DocumentTitle from 'react-document-title';
import { Provider } from 'react-redux';
import { ConnectedRouter } from 'react-router-redux';
import PageConnector from 'Components/Page/PageConnector';
import AppRoutes from './AppRoutes';

function App({ store, history }) {
  return (
    <DocumentTitle title="Sonarr">
      <Provider store={store}>
        <ConnectedRouter history={history}>
          <PageConnector>
            <AppRoutes app={App} />
          </PageConnector>
        </ConnectedRouter>
      </Provider>
    </DocumentTitle>
  );
}

App.propTypes = {
  store: PropTypes.object.isRequired,
  history: PropTypes.object.isRequired
};

export default App;
