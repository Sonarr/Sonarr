import React from 'react';
import translate from 'Utilities/String/translate';
import FilterBuilderRowValue, {
  FilterBuilderRowValueProps,
} from './FilterBuilderRowValue';

const statusTagList = [
  {
    id: 'queued',
    get name() {
      return translate('Queued');
    },
  },
  {
    id: 'paused',
    get name() {
      return translate('Paused');
    },
  },
  {
    id: 'downloading',
    get name() {
      return translate('Downloading');
    },
  },
  {
    id: 'completed',
    get name() {
      return translate('Completed');
    },
  },
  {
    id: 'failed',
    get name() {
      return translate('Failed');
    },
  },
  {
    id: 'warning',
    get name() {
      return translate('Warning');
    },
  },
  {
    id: 'delay',
    get name() {
      return translate('Delay');
    },
  },
  {
    id: 'downloadClientUnavailable',
    get name() {
      return translate('DownloadClientUnavailable');
    },
  },
  {
    id: 'fallback',
    get name() {
      return translate('Fallback');
    },
  },
];

type QueueStatusFilterBuilderRowValueProps<T> = Omit<
  FilterBuilderRowValueProps<T, string>,
  'tagList'
>;

function QueueStatusFilterBuilderRowValue<T>(
  props: QueueStatusFilterBuilderRowValueProps<T>
) {
  return <FilterBuilderRowValue {...props} tagList={statusTagList} />;
}

export default QueueStatusFilterBuilderRowValue;
