import React from 'react';
import PageContentBody from 'Components/Page/PageContentBody';
import SettingsPage from 'Settings/SettingsPage';
import translate from 'Utilities/String/translate';
import Metadatas from './Metadata/Metadatas';

function MetadataSettings() {
  return (
    <SettingsPage title={translate('MetadataSettings')} showSave={false}>
      <PageContentBody>
        <Metadatas />
      </PageContentBody>
    </SettingsPage>
  );
}

export default MetadataSettings;
