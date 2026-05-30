import React from 'react';
import PageContentBody from 'Components/Page/PageContentBody';
import SettingsPage from 'Settings/SettingsPage';
import translate from 'Utilities/String/translate';
import AutoTaggings from './AutoTagging/AutoTaggings';
import Tags from './Tags';

function TagSettings() {
  return (
    <SettingsPage title={translate('Tags')} showSave={false}>
      <PageContentBody>
        <Tags />
        <AutoTaggings />
      </PageContentBody>
    </SettingsPage>
  );
}

export default TagSettings;
