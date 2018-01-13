import React from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import TagsConnector from './TagsConnector';

function TagSettings() {
  return (
    <PageContent title="Tags">
      <SettingsToolbarConnector
        showSave={false}
      />

      <PageContentBodyConnector>
        <TagsConnector />
      </PageContentBodyConnector>
    </PageContent>
  );
}

export default TagSettings;
