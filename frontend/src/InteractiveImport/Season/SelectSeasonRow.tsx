import React, { useCallback } from 'react';
import Link from 'Components/Link/Link';
import translate from 'Utilities/String/translate';
import styles from './SelectSeasonRow.css';

interface SelectSeasonRowProps {
  seasonNumber: number;
  onSeasonSelect(season: number): unknown;
}

function SelectSeasonRow(props: SelectSeasonRowProps) {
  const { seasonNumber, onSeasonSelect } = props;

  const onSeasonSelectWrapper = useCallback(() => {
    onSeasonSelect(seasonNumber);
  }, [seasonNumber, onSeasonSelect]);

  return (
    <Link
      className={styles.season}
      component="div"
      onPress={onSeasonSelectWrapper}
    >
      {seasonNumber === 0
        ? translate('Specials')
        : translate('SeasonNumberToken', { seasonNumber })}
    </Link>
  );
}

export default SelectSeasonRow;
