import React, { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import Alert from 'Components/Alert';
import Icon from 'Components/Icon';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Column from 'Components/Table/Column';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { icons, kinds } from 'Helpers/Props';
import {
  clearEpisodeHistory,
  episodeHistoryMarkAsFailed,
  fetchEpisodeHistory,
} from 'Store/Actions/episodeHistoryActions';
import translate from 'Utilities/String/translate';
import EpisodeHistoryRow from './EpisodeHistoryRow';

const columns: Column[] = [
  {
    name: 'eventType',
    label: '',
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

interface EpisodeHistoryProps {
  episodeId: number;
}

function EpisodeHistory({ episodeId }: EpisodeHistoryProps) {
  const dispatch = useDispatch();
  const { items, isFetching, isPopulated, error } = useSelector(
    (state: AppState) => state.episodeHistory
  );

  const handleMarkAsFailedPress = useCallback(
    (historyId: number) => {
      dispatch(episodeHistoryMarkAsFailed({ historyId, episodeId }));
    },
    [episodeId, dispatch]
  );

  const hasItems = !!items.length;

  useEffect(() => {
    dispatch(fetchEpisodeHistory({ episodeId }));

    return () => {
      dispatch(clearEpisodeHistory());
    };
  }, [episodeId, dispatch]);

  if (isFetching) {
    return <LoadingIndicator />;
  }

  if (!isFetching && !!error) {
    return (
      <Alert kind={kinds.DANGER}>{translate('EpisodeHistoryLoadError')}</Alert>
    );
  }

  if (isPopulated && !hasItems && !error) {
    return <Alert kind={kinds.INFO}>{translate('NoEpisodeHistory')}</Alert>;
  }

  if (isPopulated && hasItems && !error) {
    return (
      <Table columns={columns}>
        <TableBody>
          {items.map((item) => {
            return (
              <EpisodeHistoryRow
                key={item.id}
                {...item}
                onMarkAsFailedPress={handleMarkAsFailedPress}
              />
            );
          })}
        </TableBody>
      </Table>
    );
  }

  return null;
}

export default EpisodeHistory;
