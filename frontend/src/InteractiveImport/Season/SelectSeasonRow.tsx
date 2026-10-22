import React, { useCallback } from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './SelectSeasonRow.module.css';

interface SelectSeasonRowProps {
  seasonNumber: number;
  isSelected?: boolean;
  onSeasonSelect(season: number): unknown;
}

function SelectSeasonRow(props: SelectSeasonRowProps) {
  const { seasonNumber, isSelected, onSeasonSelect } = props;

  const onSeasonSelectWrapper = useCallback(() => {
    onSeasonSelect(seasonNumber);
  }, [seasonNumber, onSeasonSelect]);

  return (
    <Link
      className={styles.season}
      component="div"
      onPress={onSeasonSelectWrapper}
    >
      {isSelected === undefined ? null : (
        <Icon
          className={styles.selectIcon}
          name={isSelected ? icons.CHECK_SQUARE : icons.SQUARE}
        />
      )}
      {seasonNumber === 0
        ? translate('Specials')
        : translate('SeasonNumberToken', { seasonNumber })}
    </Link>
  );
}

export default SelectSeasonRow;
