import React from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import AutoTaggings from './AutoTagging/AutoTaggings';
import TagsConnector from './TagsConnector';

function TagSettings() {
  return (
    <PageContent title="Tags">
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
