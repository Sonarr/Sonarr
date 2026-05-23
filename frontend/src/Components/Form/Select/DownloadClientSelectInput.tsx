import React, { useMemo } from 'react';
import DownloadProtocol from 'DownloadClient/DownloadProtocol';
import { useDownloadClients } from 'Settings/DownloadClients/DownloadClients/useDownloadClients';
import { EnhancedSelectInputChanged } from 'typings/inputs';
import sortByProp from 'Utilities/Array/sortByProp';
import translate from 'Utilities/String/translate';
import EnhancedSelectInput, {
  EnhancedSelectInputProps,
  EnhancedSelectInputValue,
} from './EnhancedSelectInput';

export interface DownloadClientSelectInputProps
  extends Omit<
    EnhancedSelectInputProps<EnhancedSelectInputValue<number>, number>,
    'values'
  > {
  name: string;
  value: number;
  includeAny?: boolean;
  protocol?: DownloadProtocol;
  onChange: (change: EnhancedSelectInputChanged<number>) => void;
}

function DownloadClientSelectInput({
  includeAny = false,
  protocol = 'torrent',
  ...otherProps
}: DownloadClientSelectInputProps) {
  const { isFetching, data } = useDownloadClients();

  const values = useMemo(() => {
    const filtered = data.filter((item) => item.protocol === protocol);

    const sorted = [...filtered]
      .sort(sortByProp('name'))
      .map((downloadClient) => ({
        key: downloadClient.id,
        value: downloadClient.name,
        hint: `(${downloadClient.id})`,
      }));

    if (includeAny) {
      sorted.unshift({
        key: 0,
        value: `(${translate('Any')})`,
        hint: '',
      });
    }

    return sorted;
  }, [data, protocol, includeAny]);

  return (
    <EnhancedSelectInput
      {...otherProps}
      isFetching={isFetching}
      values={values}
    />
  );
}

export default DownloadClientSelectInput;
