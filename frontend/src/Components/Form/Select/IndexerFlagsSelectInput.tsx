import React, { useCallback, useMemo } from 'react';
import useIndexerFlags from 'Settings/Indexers/useIndexerFlags';
import { EnhancedSelectInputChanged } from 'typings/inputs';
import EnhancedSelectInput from './EnhancedSelectInput';

export interface IndexerFlagsSelectInputProps {
  name: string;
  indexerFlags: number;
  onChange(payload: EnhancedSelectInputChanged<number>): void;
}

function IndexerFlagsSelectInput({
  name,
  indexerFlags,
  onChange,
  ...otherProps
}: IndexerFlagsSelectInputProps) {
  const { data: allIndexerFlags } = useIndexerFlags();

  const value = useMemo(
    () =>
      allIndexerFlags.reduce((acc: number[], { id }) => {
        // eslint-disable-next-line no-bitwise
        if ((indexerFlags & id) === id) {
          acc.push(id);
        }

        return acc;
      }, []),
    [allIndexerFlags, indexerFlags]
  );

  const values = useMemo(
    () =>
      allIndexerFlags.map(({ id, name }) => ({
        key: id,
        value: name,
      })),
    [allIndexerFlags]
  );

  const handleChange = useCallback(
    (change: EnhancedSelectInputChanged<number[]>) => {
      const indexerFlags = change.value.reduce(
        (acc, flagId) => acc + flagId,
        0
      );

      onChange({ name, value: indexerFlags });
    },
    [name, onChange]
  );

  return (
    <EnhancedSelectInput
      {...otherProps}
      name={name}
      value={value}
      values={values}
      onChange={handleChange}
    />
  );
}

export default IndexerFlagsSelectInput;
