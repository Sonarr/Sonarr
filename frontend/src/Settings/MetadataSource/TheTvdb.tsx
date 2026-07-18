import React from 'react';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import SectionHeading from 'Components/SectionHeading';
import useTheme from 'Helpers/Hooks/useTheme';
import translate from 'Utilities/String/translate';
import styles from './TheTvdb.css';

function TheTvdb() {
  const theme = useTheme();

  return (
    <div className={styles.container}>
      <img
        className={styles.image}
        src={`${window.Sonarr.urlBase}/Content/Images/thetvdb-${theme}.png`}
      />

      <div className={styles.info}>
        <SectionHeading
          title={translate('TheTvdb')}
          description={
            <InlineMarkdown
              data={translate(
                'SeriesAndEpisodeInformationIsProvidedByTheTVDB',
                {
                  url: 'https://www.thetvdb.com/subscribe',
                }
              )}
            />
          }
        />
      </div>
    </div>
  );
}

export default TheTvdb;
