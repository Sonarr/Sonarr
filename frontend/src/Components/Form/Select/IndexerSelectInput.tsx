import React, { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import { fetchIndexers } from 'Store/Actions/settingsActions';
import { EnhancedSelectInputChanged } from 'typings/inputs';
import sortByProp from 'Utilities/Array/sortByProp';
import translate from 'Utilities/String/translate';
import EnhancedSelectInput from './EnhancedSelectInput';

function createIndexersSelector(includeAny: boolean) {
  return createSelector(
    (state: AppState) => state.settings.indexers,
    (indexers) => {
      const { isFetching, isPopulated, error, items } = indexers;

      const values = items.sort(sortByProp('name')).map((indexer) => {
        return {
          key: indexer.id,
          value: indexer.name,
        };
      });

      if (includeAny) {
        values.unshift({
          key: 0,
          value: `(${translate('Any')})`,
        });
      }

      return {
        isFetching,
        isPopulated,
        error,
        values,
      };
    }
  );
}

export interface IndexerSelectInputProps {
  name: string;
  value: number;
  includeAny?: boolean;
  onChange: (change: EnhancedSelectInputChanged<number>) => void;
}

function IndexerSelectInput({
  name,
  value,
  includeAny = false,
  onChange,
}: IndexerSelectInputProps) {
  const dispatch = useDispatch();
  const { isFetching, isPopulated, values } = useSelector(
    createIndexersSelector(includeAny)
  );

  useEffect(() => {
    if (!isPopulated) {
      dispatch(fetchIndexers());
    }
  }, [isPopulated, dispatch]);

  return (
    <EnhancedSelectInput
      name={name}
      value={value}
      isFetching={isFetching}
      values={values}
      onChange={onChange}
    />
  );
}

export default IndexerSelectInput;
