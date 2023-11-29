import React from 'react';
import Link from 'Components/Link/Link';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import translate from 'Utilities/String/translate';
import SettingsToolbarConnector from './SettingsToolbarConnector';
import styles from './Settings.css';

function Settings() {
  return (
    <PageContent title={translate('Settings')}>
      <SettingsToolbarConnector
        hasPendingChanges={false}
      />

      <PageContentBody>
        <Link
          className={styles.link}
          to="/settings/mediamanagement"
        >
          {translate('MediaManagement')}
        </Link>

        <div className={styles.summary}>
          {translate('MediaManagementSettingsSummary')}
        </div>

        <Link
          className={styles.link}
          to="/settings/profiles"
        >
          {translate('Profiles')}
        </Link>

        <div className={styles.summary}>
          {translate('ProfilesSettingsSummary')}
        </div>

        <Link
          className={styles.link}
          to="/settings/quality"
        >
          {translate('Quality')}
        </Link>

        <div className={styles.summary}>
          {translate('QualitySettingsSummary')}
        </div>

        <Link
          className={styles.link}
          to="/settings/customformats"
        >
          {translate('CustomFormats')}
        </Link>

        <div className={styles.summary}>
          {translate('CustomFormatsSettingsSummary')}
        </div>

        <Link
          className={styles.link}
          to="/settings/indexers"
        >
          {translate('Indexers')}
        </Link>

        <div className={styles.summary}>
          {translate('IndexersSettingsSummary')}
        </div>

        <Link
          className={styles.link}
          to="/settings/downloadclients"
        >
          {translate('DownloadClients')}
        </Link>

        <div className={styles.summary}>
          {translate('DownloadClientsSettingsSummary')}
        </div>

        <Link
          className={styles.link}
          to="/settings/importlists"
        >
          {translate('ImportLists')}
        </Link>

        <div className={styles.summary}>
          {translate('ImportListsSettingsSummary')}
        </div>

        <Link
          className={styles.link}
          to="/settings/connect"
        >
          {translate('Connect')}
        </Link>

        <div className={styles.summary}>
          {translate('ConnectSettingsSummary')}
        </div>

        <Link
          className={styles.link}
          to="/settings/metadata"
        >
          {translate('Metadata')}
        </Link>

        <div className={styles.summary}>
          {translate('MetadataSettingsSeriesSummary')}
        </div>

        <Link
          className={styles.link}
          to="/settings/metadatasource"
        >
          {translate('MetadataSource')}
        </Link>

        <div className={styles.summary}>
          {translate('MetadataSourceSettingsSeriesSummary')}
        </div>

        <Link
          className={styles.link}
          to="/settings/tags"
        >
          {translate('Tags')}
        </Link>

        <div className={styles.summary}>
          {translate('TagsSettingsSummary')}
        </div>

        <Link
          className={styles.link}
          to="/settings/general"
        >
          {translate('General')}
        </Link>

        <div className={styles.summary}>
          {translate('GeneralSettingsSummary')}
        </div>

        <Link
          className={styles.link}
          to="/settings/ui"
        >
          {translate('Ui')}
        </Link>

        <div className={styles.summary}>
          {translate('UiSettingsSummary')}
        </div>
      </PageContentBody>
    </PageContent>
  );
}

Settings.propTypes = {
};

export default Settings;
