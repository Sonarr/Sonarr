import React from 'react';
import Link from 'Components/Link/Link';
import styles from './TheTvdb.css';

function TheTvdb(props) {
  return (
    <div className={styles.container}>
      <img
        className={styles.image}
        src={`${window.Sonarr.urlBase}/Content/Images/thetvdb.png`}
      />

      <div className={styles.info}>
        <div className={styles.title}>
          TheTVDB
        </div>

        <div>
          Series and episode information is provided by TheTVDB.com. <Link to="https://www.thetvdb.com/subscribe">Please consider supporting them.</Link>
        </div>
      </div>

    </div>
  );
}

export default TheTvdb;
