import React from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import SettingsToolbar from 'Settings/SettingsToolbar';
import translate from 'Utilities/String/translate';
import AutoTaggings from './AutoTagging/AutoTaggings';
import Tags from './Tags';

function TagSettings() {
  return (
    <PageContent title={translate('Tags')}>
      <SettingsToolbar showSave={false} />

      <PageContentBody>
        <Tags />
        <AutoTaggings />
      </PageContentBody>
    </PageContent>
  );
}

export default TagSettings;
