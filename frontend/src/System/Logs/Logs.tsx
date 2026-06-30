import React from 'react';
import { Route } from 'react-router-dom';
import Switch from 'Components/Router/Switch';
import AppLogFiles from './App/AppLogFiles';
import UpdateLogFiles from './Update/UpdateLogFiles';

function Logs() {
  return (
    <Switch>
      <Route index={true} element={<AppLogFiles />} />

      <Route path="update" element={<UpdateLogFiles />} />
    </Switch>
  );
}

export default Logs;
