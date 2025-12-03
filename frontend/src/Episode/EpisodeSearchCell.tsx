import React, { useCallback } from 'react';
import CommandNames from 'Commands/CommandNames';
import { useCommandExecuting, useExecuteCommand } from 'Commands/useCommands';
import IconButton from 'Components/Link/IconButton';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import { EpisodeEntity } from 'Episode/useEpisode';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import EpisodeDetailsModal from './EpisodeDetailsModal';
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
  const isSearching = useCommandExecuting(CommandNames.EpisodeSearch, {
    episodeIds: [episodeId],
  });

  const executeCommand = useExecuteCommand();

  const [isDetailsModalOpen, setDetailsModalOpen, setDetailsModalClosed] =
    useModalOpenState(false);

  const handleSearchPress = useCallback(() => {
    executeCommand({
      name: CommandNames.EpisodeSearch,
      episodeIds: [episodeId],
    });
  }, [episodeId, executeCommand]);

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
    </TableRowCell>
  );
}

export default EpisodeSearchCell;
