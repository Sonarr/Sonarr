import classNames from 'classnames';
import React, { useCallback } from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons } from 'Helpers/Props';
import useExistingSeries from 'Series/useExistingSeries';
import ImportSeriesTitle from './ImportSeriesTitle';
import styles from './ImportSeriesSearchResult.css';

interface ImportSeriesSearchResultProps {
  id: string;
  tvdbId: number;
  title: string;
  year: number;
  network?: string;
  isHighlighted: boolean;
  onPress: (tvdbId: number) => void;
}

function ImportSeriesSearchResult({
  id,
  tvdbId,
  title,
  year,
  network,
  isHighlighted,
  onPress,
}: ImportSeriesSearchResultProps) {
  const isExistingSeries = useExistingSeries(tvdbId);

  const handlePress = useCallback(() => {
    onPress(tvdbId);
  }, [tvdbId, onPress]);

  return (
    <div
      className={classNames(
        styles.container,
        isHighlighted ? styles.highlighted : undefined
      )}
      role="option"
      id={id}
      aria-selected={isHighlighted}
    >
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
