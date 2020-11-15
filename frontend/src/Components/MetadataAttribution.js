import React from 'react';
import Link from 'Components/Link/Link';
import styles from './MetadataAttribution.css';

export default function MetadataAttribution() {
  return (
    <div className={styles.container}>
      <Link
        className={styles.attribution}
        to="/settings/metadatasource"
      >
        Metadata is provided by TheTVDB
      </Link>
    </div>
  );
}
