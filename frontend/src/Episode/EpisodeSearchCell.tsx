import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { EPISODE_SEARCH } from 'Commands/commandNames';
import IconButton from 'Components/Link/IconButton';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import { EpisodeEntity } from 'Episode/useEpisode';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import { icons } from 'Helpers/Props';
import { executeCommand } from 'Store/Actions/commandActions';
import createExecutingCommandsSelector from 'Store/Selectors/createExecutingCommandsSelector';
import useSeries from 'Series/useSeries';
import translate from 'Utilities/String/translate';
import EpisodeDetailsModal from './EpisodeDetailsModal';
import ManualSearchModal from './Search/ManualSearchModal';
import styles from './EpisodeSearchCell.css';

interface EpisodeSearchCellProps {
  episodeId: number;
  episodeEntity: EpisodeEntity;
  seriesId: number;
  episodeTitle: string;
  showOpenSeriesButton: boolean;
}

function EpisodeSearchCell({
  episodeId,
  episodeEntity,
  seriesId,
  episodeTitle,
  showOpenSeriesButton,
}: EpisodeSearchCellProps) {
  const series = useSeries(seriesId);
  const enableManualSearch = series?.enableManualSearch ?? false;
  
  const executingCommands = useSelector(createExecutingCommandsSelector());
  const isSearching = executingCommands.some(({ name, body }) => {
    const { episodeIds = [] } = body;
    return name === EPISODE_SEARCH && episodeIds.indexOf(episodeId) > -1;
  });

  const dispatch = useDispatch();

  const [isDetailsModalOpen, setDetailsModalOpen, setDetailsModalClosed] =
    useModalOpenState(false);

  const [isManualSearchModalOpen, setManualSearchModalOpen, setManualSearchModalClosed] =
    useModalOpenState(false);

  const handleSearchPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: EPISODE_SEARCH,
        episodeIds: [episodeId],
      })
    );
  }, [episodeId, dispatch]);

  return (
    <TableRowCell className={styles.episodeSearchCell}>
      <SpinnerIconButton
        name={icons.SEARCH}
        isSpinning={isSearching}
        title={translate('AutomaticSearch')}
        onPress={handleSearchPress}
      />

      <IconButton
        name={icons.INTERACTIVE}
        title={translate('InteractiveSearch')}
        onPress={setDetailsModalOpen}
      />

      {enableManualSearch && (
        <IconButton
          name={icons.EDIT}
          title={translate('ManualSearch')}
          onPress={setManualSearchModalOpen}
        />
      )}

      <EpisodeDetailsModal
        isOpen={isDetailsModalOpen}
        episodeId={episodeId}
        episodeEntity={episodeEntity}
        seriesId={seriesId}
        episodeTitle={episodeTitle}
        selectedTab="search"
        startInteractiveSearch={true}
        showOpenSeriesButton={showOpenSeriesButton}
        onModalClose={setDetailsModalClosed}
      />

      {enableManualSearch && (
        <ManualSearchModal
          isOpen={isManualSearchModalOpen}
          episodeId={episodeId}
          episodeTitle={episodeTitle}
          onModalClose={setManualSearchModalClosed}
        />
      )}
    </TableRowCell>
  );
}

export default EpisodeSearchCell;
