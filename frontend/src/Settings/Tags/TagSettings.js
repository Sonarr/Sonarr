import React from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import translate from 'Utilities/String/translate';
import AutoTaggings from './AutoTagging/AutoTaggings';
import TagsConnector from './TagsConnector';

function TagSettings() {
  return (
    <PageContent title={translate('Tags')}>
      <SettingsToolbarConnector
        showSave={false}
      />

      <PageContentBody>
        <TagsConnector />
        <AutoTaggings />
      </PageContentBody>
    </PageContent>
  );
}

export default TagSettings;
