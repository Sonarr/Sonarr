import React from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import MetadatasConnector from './Metadata/MetadatasConnector';

function MetadataSettings() {
  return (
    <PageContent title="Metadata Settings">
      <SettingsToolbarConnector
        showSave={false}
      />

      <PageContentBodyConnector>
        <MetadatasConnector />
      </PageContentBodyConnector>
    </PageContent>
  );
}

export default MetadataSettings;
