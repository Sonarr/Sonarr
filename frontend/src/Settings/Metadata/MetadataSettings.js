import React from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import translate from 'Utilities/String/translate';
import MetadatasConnector from './Metadata/MetadatasConnector';

function MetadataSettings() {
  return (
    <PageContent title={translate('MetadataSettings')}>
      <SettingsToolbarConnector
        showSave={false}
      />

      <PageContentBody>
        <MetadatasConnector />
      </PageContentBody>
    </PageContent>
  );
}

export default MetadataSettings;
