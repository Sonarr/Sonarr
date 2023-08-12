import React from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import translate from 'Utilities/String/translate';
import TheTvdb from './TheTvdb';

function MetadataSourceSettings() {
  return (
    <PageContent title={translate('MetadataSourceSettings')} >
      <SettingsToolbarConnector
        showSave={false}
      />

      <PageContentBody>
        <TheTvdb />
      </PageContentBody>
    </PageContent>
  );
}

export default MetadataSourceSettings;
