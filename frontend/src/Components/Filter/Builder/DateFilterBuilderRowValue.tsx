import React, { useCallback, useEffect } from 'react';
import NumberInput from 'Components/Form/NumberInput';
import SelectInput from 'Components/Form/SelectInput';
import TextInput from 'Components/Form/TextInput';
import usePrevious from 'Helpers/Hooks/usePrevious';
import {
  DateFilterBuilderTime,
  DateFilterValue,
  FilterType,
} from 'Helpers/Props/filterTypes';
import { InputChanged, InputOnChange } from 'typings/inputs';
import isString from 'Utilities/String/isString';
import { FilterBuilderRowValueProps, NAME } from './FilterBuilderRowValue';
import styles from './DateFilterBuilderRowValue.css';

const timeOptions: DateFilterBuilderTime[] = [
  { key: 'seconds', value: 'seconds' },
  { key: 'minutes', value: 'minutes' },
  { key: 'hours', value: 'hours' },
  { key: 'days', value: 'days' },
  { key: 'weeks', value: 'weeks' },
  { key: 'months', value: 'months' },
];

function isInFilter(filterType: FilterType) {
  return (
    filterType === 'inLast' ||
    filterType === 'notInLast' ||
    filterType === 'inNext' ||
    filterType === 'notInNext'
  );
}

interface DateFilterBuilderRowValueProps<T>
  extends Omit<
    FilterBuilderRowValueProps<T, string>,
    'filterValue' | 'onChange'
  > {
  filterValue: string | DateFilterValue;
  onChange: InputOnChange<string | DateFilterValue>;
}

function DateFilterBuilderRowValue<T>({
  filterType,
  filterValue,
  onChange,
}: DateFilterBuilderRowValueProps<T>) {
  const previousFilterType = usePrevious(filterType);

  const handleValueChange = useCallback(
    ({ value }: InputChanged<number | null>) => {
      onChange({
        name: NAME,
        value: {
          time: (filterValue as DateFilterValue).time,
          value,
        },
      });
    },
    [filterValue, onChange]
  );

  const handleTimeChange = useCallback(
    ({ value }: InputChanged<string>) => {
      onChange({
        name: NAME,
        value: {
          time: value,
          value: (filterValue as DateFilterValue).value,
        },
      });
    },
    [filterValue, onChange]
  );

  const handleDateChange = useCallback(
    ({ value }: InputChanged<string>) => {
      onChange({
        name: NAME,
        value,
      });
    },
    [onChange]
  );

  useEffect(() => {
    if (previousFilterType === filterType) {
      return;
    }

    if (isInFilter(filterType) && isString(filterValue)) {
      onChange({
        name: NAME,
        value: {
          time: timeOptions[0].key,
          value: null,
        },
      });

      return;
    }

    if (!isInFilter(filterType) && !isString(filterValue)) {
      onChange({
        name: NAME,
        value: '',
      });
    }
  }, [filterType, previousFilterType, filterValue, onChange]);

  if (
    (isInFilter(filterType) && isString(filterValue)) ||
    (!isInFilter(filterType) && !isString(filterValue))
  ) {
    return null;
  }

  if (isInFilter(filterType)) {
    const { value, time } = filterValue as DateFilterValue;

    return (
      <div className={styles.container}>
        <NumberInput
          className={styles.numberInput}
          name={NAME}
          value={value}
          onChange={handleValueChange}
        />

        <SelectInput
          className={styles.selectInput}
          name={NAME}
          value={time}
          values={timeOptions}
          onChange={handleTimeChange}
        />
      </div>
    );
  }

  return (
    <TextInput
      name={NAME}
      value={filterValue as string}
      type="date"
      placeholder="yyyy-mm-dd"
      onChange={handleDateChange}
    />
  );
}

export default DateFilterBuilderRowValue;
