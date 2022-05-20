import React, { Component } from 'react';
import { Route } from 'react-router-dom';
import ImportSeriesConnector from 'AddSeries/ImportSeries/Import/ImportSeriesConnector';
import ImportSeriesSelectFolderConnector from 'AddSeries/ImportSeries/SelectFolder/ImportSeriesSelectFolderConnector';
import Switch from 'Components/Router/Switch';

class ImportSeries extends Component {

  //
  // Render

  render() {
    return (
      <Switch>
        <Route
          exact={true}
          path="/add/import"
          component={ImportSeriesSelectFolderConnector}
        />

        <Route
          path="/add/import/:rootFolderId"
          component={ImportSeriesConnector}
        />
      </Switch>
    );
  }
}

export default ImportSeries;
