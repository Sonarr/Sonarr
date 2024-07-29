import React from 'react';
import Link from 'Components/Link/Link';
import translate from 'Utilities/String/translate';
import styles from './MetadataAttribution.css';

export default function MetadataAttribution() {
  return (
    <div className={styles.container}>
      <Link className={styles.attribution} to="/settings/metadatasource">
        {translate('MetadataProvidedBy', { provider: 'TheTVDB' })}
      </Link>
    </div>
  );
}
