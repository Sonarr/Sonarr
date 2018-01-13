import React from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import NotificationsConnector from './Notifications/NotificationsConnector';

function NotificationSettings() {
  return (
    <PageContent title="Connect Settings">
      <SettingsToolbarConnector
        showSave={false}
      />

      <PageContentBodyConnector>
        <NotificationsConnector />
      </PageContentBodyConnector>
    </PageContent>
  );
}

export default NotificationSettings;
