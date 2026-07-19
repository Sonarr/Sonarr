import React, { ReactNode, useMemo } from 'react';
import { EnhancedSelectInputValue } from 'Components/Form/Select/EnhancedSelectInput';
import Icon from 'Components/Icon';
import { icons } from 'Helpers/Props';
import styles from './ImportSeriesChip.css';

interface ImportSeriesChipProps<T extends EnhancedSelectInputValue<V>, V> {
  values: T[];
  selectedValue: V;
  isDisabled?: boolean;
  label: string;
  showDot?: boolean;
  isOverride?: boolean;
}

function ImportSeriesChip<T extends EnhancedSelectInputValue<V>, V>({
  values,
  selectedValue,
  isDisabled = false,
  label,
  showDot = false,
  isOverride = false,
}: ImportSeriesChipProps<T, V>) {
  const match = values.find((v) => v.key === selectedValue);
  const valueText = (match?.value ?? selectedValue) as ReactNode;

  const containerClass = useMemo(() => {
    if (isDisabled) return styles.chipDisabled;
    if (isOverride) return styles.chipOverride;

    return styles.chip;
  }, [isDisabled, isOverride]);

  return (
    <div className={containerClass}>
      {showDot ? <span className={styles.dot} /> : null}

      <span className={isOverride ? styles.labelOverride : styles.label}>
        {label}
      </span>

      <span className={styles.value}>{valueText}</span>

      <Icon className={styles.caret} name={icons.CARET_DOWN} size={14} />
    </div>
  );
}

export default ImportSeriesChip;
