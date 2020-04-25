import React from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import TagsConnector from './TagsConnector';

function TagSettings() {
  return (
    <PageContent title="Tags">
      <SettingsToolbarConnector
        showSave={false}
      />

      <PageContentBody>
        <TagsConnector />
      </PageContentBody>
    </PageContent>
  );
}

export default TagSettings;
