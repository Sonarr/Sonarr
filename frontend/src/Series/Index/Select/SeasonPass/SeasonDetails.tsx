import React, { useMemo } from 'react';
import { Season } from 'Series/Series';
import translate from 'Utilities/String/translate';
import SeasonPassSeason from './SeasonPassSeason';
import styles from './SeasonDetails.css';

interface SeasonDetailsProps {
  seriesId: number;
  seasons: Season[];
}

function SeasonDetails(props: SeasonDetailsProps) {
  const { seriesId, seasons } = props;

  const latestSeasons = useMemo(() => {
    return seasons.slice(Math.max(seasons.length - 25, 0));
  }, [seasons]);

  return (
    <div className={styles.seasons}>
      {latestSeasons.map((season) => {
        const {
          seasonNumber,
          monitored,
          statistics,
          isSaving = false,
        } = season;

        return (
          <SeasonPassSeason
            key={seasonNumber}
            seriesId={seriesId}
            seasonNumber={seasonNumber}
            monitored={monitored}
            statistics={statistics}
            isSaving={isSaving}
          />
        );
      })}

      {latestSeasons.length < seasons.length ? (
        <div className={styles.truncated}>
          {translate('SeasonPassTruncated')}
        </div>
      ) : null}
    </div>
  );
}

export default SeasonDetails;
