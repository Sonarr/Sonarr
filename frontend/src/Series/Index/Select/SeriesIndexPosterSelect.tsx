import React, { useCallback } from 'react';
import { SelectActionType, useSelect } from 'App/SelectContext';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons } from 'Helpers/Props';
import styles from './SeriesIndexPosterSelect.css';

interface SeriesIndexPosterSelectProps {
  seriesId: number;
}

function SeriesIndexPosterSelect(props: SeriesIndexPosterSelectProps) {
  const { seriesId } = props;
  const [selectState, selectDispatch] = useSelect();
  const isSelected = selectState.selectedState[seriesId];

  const onSelectPress = useCallback(
    (event) => {
      const shiftKey = event.nativeEvent.shiftKey;

      selectDispatch({
        type: SelectActionType.ToggleSelected,
        id: seriesId,
        isSelected: !isSelected,
        shiftKey,
      });
    },
    [seriesId, isSelected, selectDispatch]
  );

  return (
    <Link className={styles.checkButton} onPress={onSelectPress}>
      <span className={styles.checkContainer}>
        <Icon
          className={isSelected ? styles.selected : styles.unselected}
          name={isSelected ? icons.CHECK_CIRCLE : icons.CIRCLE_OUTLINE}
          size={20}
        />
      </span>
    </Link>
  );
}

export default SeriesIndexPosterSelect;
