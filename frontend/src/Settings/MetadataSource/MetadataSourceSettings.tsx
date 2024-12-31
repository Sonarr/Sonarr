import React from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import SettingsToolbar from 'Settings/SettingsToolbar';
import translate from 'Utilities/String/translate';
import TheTvdb from './TheTvdb';

function MetadataSourceSettings() {
  return (
    <PageContent title={translate('MetadataSourceSettings')}>
      <SettingsToolbar showSave={false} />

      <PageContentBody>
        <TheTvdb />
      </PageContentBody>
    </PageContent>
  );
}

export default MetadataSourceSettings;
