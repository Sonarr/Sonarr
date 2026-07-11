import React from 'react';
import PageContentBody from 'Components/Page/PageContentBody';
import PageHeading from 'Components/Page/PageHeading';
import settingsStyles from 'Settings/Settings.css';
import SettingsPage from 'Settings/SettingsPage';
import translate from 'Utilities/String/translate';
import Metadatas from './Metadata/Metadatas';

function MetadataSettings() {
  return (
    <SettingsPage title={translate('MetadataSettings')} showSave={false}>
      <PageContentBody>
        <div className={settingsStyles.section}>
          <PageHeading
            scope={`${translate('Configuration')} · ${translate('Metadata')}`}
            title={translate('MetadataSettings')}
          />

          <Metadatas />
        </div>
      </PageContentBody>
    </SettingsPage>
  );
}

export default MetadataSettings;
