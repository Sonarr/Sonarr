import React from 'react';
import { Route } from 'react-router-dom';
import Switch from 'Components/Router/Switch';
import AppLogFiles from './App/AppLogFiles';
import UpdateLogFiles from './Update/UpdateLogFiles';

function Logs() {
  return (
    <Switch>
      <Route exact={true} path="/system/logs/files" component={AppLogFiles} />

      <Route path="/system/logs/files/update" component={UpdateLogFiles} />
    </Switch>
  );
}

export default Logs;
