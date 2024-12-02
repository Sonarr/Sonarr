import React from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import translate from 'Utilities/String/translate';
import Metadatas from './Metadata/Metadatas';

function MetadataSettings() {
  return (
    <PageContent title={translate('MetadataSettings')}>
      <SettingsToolbarConnector
        showSave={false}
      />

      <PageContentBody>
        <Metadatas />
      </PageContentBody>
    </PageContent>
  );
}

export default MetadataSettings;
