import React from 'react';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import translate from 'Utilities/String/translate';
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
          {translate('TheTvdb')}
        </div>
        <InlineMarkdown data={translate('SeriesAndEpisodeInformationIsProvidedByTheTVDB', { url: 'https://www.thetvdb.com/subscribe' })} />
      </div>

    </div>
  );
}

export default TheTvdb;
