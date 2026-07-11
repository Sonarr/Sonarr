import React from 'react';
import PageContentBody from 'Components/Page/PageContentBody';
import PageHeading from 'Components/Page/PageHeading';
import settingsStyles from 'Settings/Settings.css';
import SettingsPage from 'Settings/SettingsPage';
import translate from 'Utilities/String/translate';
import AutoTaggings from './AutoTagging/AutoTaggings';
import Tags from './Tags';
import styles from './TagSettings.css';

function TagSettings() {
  return (
    <SettingsPage title={translate('Tags')} showSave={false}>
      <PageContentBody>
        <div className={settingsStyles.section}>
          <PageHeading
            scope={translate('Settings')}
            title={translate('Tags')}
          />

          <div className={styles.section}>
            <div className={styles.sectionHeaderRow}>
              <h3 className={styles.sectionHeading}>{translate('Tags')}</h3>
            </div>
            <p className={styles.sectionLede}>
              Labels you can attach to series, indexers, profiles, and download
              clients to scope behavior.
            </p>
            <Tags />
          </div>

          <div className={styles.section}>
            <div className={styles.sectionHeaderRow}>
              <h3 className={styles.sectionHeading}>
                {translate('AutoTagging')}
              </h3>
            </div>
            <p className={styles.sectionLede}>
              Rules that apply tags to series automatically based on conditions
              like genre, network, or year.
            </p>
            <AutoTaggings />
          </div>
        </div>
      </PageContentBody>
    </SettingsPage>
  );
}

export default TagSettings;
