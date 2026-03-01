import React, { useCallback } from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons } from 'Helpers/Props';
import useExistingSeries from 'Series/useExistingSeries';
import ImportSeriesTitle from './ImportSeriesTitle';
import styles from './ImportSeriesSearchResult.css';

interface ImportSeriesSearchResultProps {
  tvdbId: number;
  title: string;
  year: number;
  network?: string;
  onPress: (tvdbId: number) => void;
}

function ImportSeriesSearchResult({
  tvdbId,
  title,
  year,
  network,
  onPress,
}: ImportSeriesSearchResultProps) {
  const isExistingSeries = useExistingSeries(tvdbId);

  const handlePress = useCallback(() => {
    onPress(tvdbId);
  }, [tvdbId, onPress]);

  return (
    <div className={styles.container}>
      <Link className={styles.series} onPress={handlePress}>
        <ImportSeriesTitle
          title={title}
          year={year}
          network={network}
          isExistingSeries={isExistingSeries}
        />
      </Link>

      <Link
        className={styles.tvdbLink}
        to={`https://www.thetvdb.com/?tab=series&id=${tvdbId}`}
      >
        <Icon
          className={styles.tvdbLinkIcon}
          name={icons.EXTERNAL_LINK}
          size={16}
        />
      </Link>
    </div>
  );
}

export default ImportSeriesSearchResult;
