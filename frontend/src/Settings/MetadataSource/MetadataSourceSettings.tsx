import React from 'react';
import PageContentBody from 'Components/Page/PageContentBody';
import SettingsPage from 'Settings/SettingsPage';
import translate from 'Utilities/String/translate';
import TheTvdb from './TheTvdb';

function MetadataSourceSettings() {
  return (
    <SettingsPage title={translate('MetadataSourceSettings')} showSave={false}>
      <PageContentBody>
        <TheTvdb />
      </PageContentBody>
    </SettingsPage>
  );
}

export default MetadataSourceSettings;
