import React from 'react';
import Link from 'Components/Link/Link';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import SettingsToolbarConnector from './SettingsToolbarConnector';
import styles from './Settings.css';

function Settings() {
  return (
    <PageContent title="Settings">
      <SettingsToolbarConnector
        hasPendingChanges={false}
      />

      <PageContentBodyConnector>
        <Link
          className={styles.link}
          to="/settings/mediamanagement"
        >
          Media Management
        </Link>

        <div className={styles.summary}>
          Naming and file management settings
        </div>

        <Link
          className={styles.link}
          to="/settings/profiles"
        >
          Profiles
        </Link>

        <div className={styles.summary}>
          Quality, Language and Delay profiles
        </div>

        <Link
          className={styles.link}
          to="/settings/quality"
        >
          Quality
        </Link>

        <div className={styles.summary}>
          Quality sizes and naming
        </div>

        <Link
          className={styles.link}
          to="/settings/indexers"
        >
          Indexers
        </Link>

        <div className={styles.summary}>
          Indexers and release restrictions
        </div>

        <Link
          className={styles.link}
          to="/settings/downloadclients"
        >
          Download Clients
        </Link>

        <div className={styles.summary}>
          Download clients, download handling and remote path mappings
        </div>

        <Link
          className={styles.link}
          to="/settings/connect"
        >
          Connect
        </Link>

        <div className={styles.summary}>
          Notifications, connections to media servers/players and custom scripts
        </div>

        <Link
          className={styles.link}
          to="/settings/metadata"
        >
          Metadata
        </Link>

        <div className={styles.summary}>
          Create metadata files when episodes are imported or series are refreshed
        </div>

        <Link
          className={styles.link}
          to="/settings/tags"
        >
          Tags
        </Link>

        <div className={styles.summary}>
          See all tags and how they are used. Unused tags can be removed
        </div>

        <Link
          className={styles.link}
          to="/settings/general"
        >
          General
        </Link>

        <div className={styles.summary}>
          Port, SSL, username/password, proxy, analytics and updates
        </div>

        <Link
          className={styles.link}
          to="/settings/ui"
        >
          UI
        </Link>

        <div className={styles.summary}>
          Calendar, date and color impaired options
        </div>
      </PageContentBodyConnector>
    </PageContent>
  );
}

Settings.propTypes = {
};

export default Settings;
