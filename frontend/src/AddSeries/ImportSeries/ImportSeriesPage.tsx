import React from 'react';
import { Route } from 'react-router-dom';
import Switch from 'Components/Router/Switch';
import ImportSeries from './Import/ImportSeries';
import ImportSeriesSelectFolder from './SelectFolder/ImportSeriesSelectFolder';

function ImportSeriesPage() {
  return (
    <Switch>
      <Route path="/add/import" element={<ImportSeriesSelectFolder />} />

      <Route path="/add/import/:rootFolderId" element={<ImportSeries />} />
    </Switch>
  );
}

export default ImportSeriesPage;
