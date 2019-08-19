import React, { Component } from 'react';
import { Route, Redirect } from 'react-router-dom';
import getPathWithUrlBase from 'Utilities/getPathWithUrlBase';
import Switch from 'Components/Router/Switch';
import StatusConnector from './Status/StatusConnector';
import ScriptConnector from './Script/ScriptConnector';

class Diagnostic extends Component {

  //
  // Render

  render() {
    return (
      <Switch>
        <Route
          exact={true}
          path="/diag/status"
          component={StatusConnector}
        />
        <Route
          exact={true}
          path="/diag/script"
          component={ScriptConnector}
        />

        {/* Redirect root to status */}
        <Route
          exact={true}
          path="/diag"
          render={() => {
            return (
              <Redirect
                to={getPathWithUrlBase('/diag/status')}
              />
            );
          }}
        />
      </Switch>
    );
  }
}

export default Diagnostic;
