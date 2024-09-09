import React, { ReactNode, useMemo } from 'react';
import Label from 'Components/Label';
import ArrayElement from 'typings/Helpers/ArrayElement';
import { EnhancedSelectInputValue } from './EnhancedSelectInput';
import EnhancedSelectInputSelectedValue from './EnhancedSelectInputSelectedValue';
import styles from './HintedSelectInputSelectedValue.css';

interface HintedSelectInputSelectedValueProps<T, V> {
  selectedValue: V;
  values: T[];
  hint?: ReactNode;
  isMultiSelect?: boolean;
  includeHint?: boolean;
}

function HintedSelectInputSelectedValue<
  T extends EnhancedSelectInputValue<V>,
  V extends number | string
>(props: HintedSelectInputSelectedValueProps<T, V>) {
  const {
    selectedValue,
    values,
    hint,
    isMultiSelect = false,
    includeHint = true,
    ...otherProps
  } = props;

  const valuesMap = useMemo(() => {
    return new Map(values.map((v) => [v.key, v.value]));
  }, [values]);

  return (
    <EnhancedSelectInputSelectedValue
      className={styles.selectedValue}
      {...otherProps}
    >
      <div className={styles.valueText}>
        {isMultiSelect && Array.isArray(selectedValue)
          ? selectedValue.map((key) => {
              const v = valuesMap.get(key);

              return <Label key={key}>{v ? v : key}</Label>;
            })
          : valuesMap.get(selectedValue as ArrayElement<V>)}
      </div>

      {hint != null && includeHint ? (
        <div className={styles.hintText}>{hint}</div>
      ) : null}
    </EnhancedSelectInputSelectedValue>
  );
}

export default HintedSelectInputSelectedValue;
