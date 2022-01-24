import React from 'react';
import Link from 'Components/Link/Link';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import SettingsToolbarConnector from './SettingsToolbarConnector';
import styles from './Settings.css';

function Settings() {
  return (
    <PageContent title="Settings">
      <SettingsToolbarConnector
        hasPendingChanges={false}
      />

      <PageContentBody>
        <Link
          className={styles.link}
          to="/settings/mediamanagement"
        >
          Media Management
        </Link>

        <div className={styles.summary}>
          Naming, file management settings and root folders
        </div>

        <Link
          className={styles.link}
          to="/settings/profiles"
        >
          Profiles
        </Link>

        <div className={styles.summary}>
          Quality, Language, Delay and Release profiles
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
          to="/settings/customformats"
        >
          Custom Formats
        </Link>

        <div className={styles.summary}>
          Custom Formats and Settings
        </div>

        <Link
          className={styles.link}
          to="/settings/indexers"
        >
          Indexers
        </Link>

        <div className={styles.summary}>
          Indexers and indexer options
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
          to="/settings/importlists"
        >
          Import Lists
        </Link>

        <div className={styles.summary}>
          Import from another Sonarr instance or Trakt lists and manage list exclusions
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
      </PageContentBody>
    </PageContent>
  );
}

Settings.propTypes = {
};

export default Settings;
