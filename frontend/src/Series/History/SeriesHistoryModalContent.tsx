import React from 'react';
import useSeriesHistory from 'Activity/History/useSeriesHistory';
import Alert from 'Components/Alert';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Column from 'Components/Table/Column';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { icons, kinds } from 'Helpers/Props';
import formatSeason from 'Season/formatSeason';
import translate from 'Utilities/String/translate';
import SeriesHistoryRow from './SeriesHistoryRow';

const columns: Column[] = [
  {
    name: 'eventType',
    label: '',
    isVisible: true,
  },
  {
    name: 'episode',
    label: () => translate('Episode'),
    isVisible: true,
  },
  {
    name: 'sourceTitle',
    label: () => translate('SourceTitle'),
    isVisible: true,
  },
  {
    name: 'languages',
    label: () => translate('Languages'),
    isVisible: true,
  },
  {
    name: 'quality',
    label: () => translate('Quality'),
    isVisible: true,
  },
  {
    name: 'customFormats',
    label: () => translate('CustomFormats'),
    isSortable: false,
    isVisible: true,
  },
  {
    name: 'customFormatScore',
    label: React.createElement(Icon, {
      name: icons.SCORE,
      title: () => translate('CustomFormatScore'),
    }),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'date',
    label: () => translate('Date'),
    isVisible: true,
  },
  {
    name: 'actions',
    label: '',
    isVisible: true,
  },
];

export interface SeriesHistoryModalContentProps {
  seriesId: number;
  seasonNumber?: number;
  onModalClose: () => void;
}

function SeriesHistoryModalContent({
  seriesId,
  seasonNumber,
  onModalClose,
}: SeriesHistoryModalContentProps) {
  const { isFetching, isFetched, error, data } = useSeriesHistory(
    seriesId,
    seasonNumber
  );

  const fullSeries = seasonNumber == null;
  const hasItems = !!data.length;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {seasonNumber == null
          ? translate('History')
          : translate('HistoryModalHeaderSeason', {
              season: formatSeason(seasonNumber)!,
            })}
      </ModalHeader>

      <ModalBody>
        {isFetching && !isFetched ? <LoadingIndicator /> : null}

        {!isFetching && !!error ? (
          <Alert kind={kinds.DANGER}>{translate('HistoryLoadError')}</Alert>
        ) : null}

        {isFetched && !hasItems && !error ? (
          <div>{translate('NoHistory')}</div>
        ) : null}

        {isFetched && hasItems && !error ? (
          <Table columns={columns}>
            <TableBody>
              {data.map((item) => {
                return (
                  <SeriesHistoryRow
                    key={item.id}
                    fullSeries={fullSeries}
                    {...item}
                  />
                );
              })}
            </TableBody>
          </Table>
        ) : null}
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Close')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default SeriesHistoryModalContent;
