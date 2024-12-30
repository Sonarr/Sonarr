import React, { useCallback } from 'react';
import NumberInput from 'Components/Form/NumberInput';
import { InputChanged } from 'typings/inputs';
import styles from './QualityProfileFormatItem.css';

interface QualityProfileFormatItemProps {
  formatId: number;
  name: string;
  score?: number;
  onScoreChange: (formatId: number, score: number) => void;
}

function QualityProfileFormatItem({
  formatId,
  name,
  score = 0,
  onScoreChange,
}: QualityProfileFormatItemProps) {
  const handleScoreChange = useCallback(
    ({ value }: InputChanged<number>) => {
      onScoreChange(formatId, value);
    },
    [formatId, onScoreChange]
  );

  return (
    <div className={styles.qualityProfileFormatItemContainer}>
      <div className={styles.qualityProfileFormatItem}>
        <label className={styles.formatNameContainer}>
          <div className={styles.formatName}>{name}</div>
          <NumberInput
            containerClassName={styles.scoreContainer}
            className={styles.scoreInput}
            name={name}
            value={score}
            // @ts-expect-error - mismatched types
            onChange={handleScoreChange}
          />
        </label>
      </div>
    </div>
  );
}

export default QualityProfileFormatItem;
