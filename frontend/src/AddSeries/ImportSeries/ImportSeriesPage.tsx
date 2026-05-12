import React from 'react';
import { Route } from 'react-router-dom';
import Switch from 'Components/Router/Switch';
import ImportSeries from './Import/ImportSeries';
import ImportSeriesSelectFolder from './SelectFolder/ImportSeriesSelectFolder';

function ImportSeriesPage() {
  return (
    <Switch>
      <Route index={true} element={<ImportSeriesSelectFolder />} />
      <Route path=":rootFolderId" element={<ImportSeries />} />
    </Switch>
  );
}

export default ImportSeriesPage;
