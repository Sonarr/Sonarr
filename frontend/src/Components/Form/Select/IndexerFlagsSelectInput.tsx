import React, { useCallback } from 'react';
import { useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import { EnhancedSelectInputChanged } from 'typings/inputs';
import EnhancedSelectInput from './EnhancedSelectInput';

const selectIndexerFlagsValues = (selectedFlags: number) =>
  createSelector(
    (state: AppState) => state.settings.indexerFlags,
    (indexerFlags) => {
      const value = indexerFlags.items.reduce((acc: number[], { id }) => {
        // eslint-disable-next-line no-bitwise
        if ((selectedFlags & id) === id) {
          acc.push(id);
        }

        return acc;
      }, []);

      const values = indexerFlags.items.map(({ id, name }) => ({
        key: id,
        value: name,
      }));

      return {
        value,
        values,
      };
    }
  );

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
  const { value, values } = useSelector(selectIndexerFlagsValues(indexerFlags));

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
