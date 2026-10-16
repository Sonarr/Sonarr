import React, { useMemo } from 'react';
import { useSortedIndexers } from 'Settings/Indexers/useIndexers';
import { EnhancedSelectInputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import EnhancedSelectInput from './EnhancedSelectInput';

export interface IndexerSelectInputProps {
  id?: string;
  name: string;
  value: number | number[];
  includeAny?: boolean;
  onChange: (change: EnhancedSelectInputChanged<number | number[]>) => void;
}

function IndexerSelectInput({
  id,
  name,
  value,
  includeAny = false,
  onChange,
}: IndexerSelectInputProps) {
  const { isFetching, data } = useSortedIndexers();

  const values = useMemo(() => {
    const indexerOptions = data.map((indexer) => ({
      key: indexer.id,
      value: indexer.name,
    }));

    if (includeAny) {
      indexerOptions.unshift({
        key: 0,
        value: `(${translate('Any')})`,
      });
    }

    return indexerOptions;
  }, [data, includeAny]);

  return (
    <EnhancedSelectInput
      id={id}
      name={name}
      value={value}
      isFetching={isFetching}
      values={values}
      onChange={onChange}
    />
  );
}

export default IndexerSelectInput;
