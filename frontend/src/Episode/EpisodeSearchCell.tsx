import React, { useCallback } from 'react';
import CommandNames from 'Commands/CommandNames';
import { useCommandExecuting, useExecuteCommand } from 'Commands/useCommands';
import IconButton from 'Components/Link/IconButton';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import { EpisodeEntity } from 'Episode/useEpisode';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import { icons } from 'Helpers/Props';
import padNumber from 'Utilities/Number/padNumber';
import translate from 'Utilities/String/translate';
import EpisodeDetailsModal from './EpisodeDetailsModal';
import styles from './EpisodeSearchCell.module.css';

interface EpisodeSearchCellProps {
  className?: string;
  component?: React.ElementType;
  episodeId: number;
  episodeEntity: EpisodeEntity;
  seriesId: number;
  seriesTitle: string;
  seasonNumber: number;
  episodeNumber: number;
  episodeTitle: string;
}

function EpisodeSearchCell({
  className = styles.episodeSearchCell,
  component: Component = TableRowCell,
  episodeId,
  episodeEntity,
  seriesId,
  seriesTitle,
  seasonNumber,
  episodeNumber,
  episodeTitle,
}: EpisodeSearchCellProps) {
  const isSearching = useCommandExecuting(CommandNames.EpisodeSearch, {
    episodeIds: [episodeId],
  });

  const executeCommand = useExecuteCommand();

  const [isDetailsModalOpen, setDetailsModalOpen, setDetailsModalClosed] =
    useModalOpenState(false);

  const episode = `${seasonNumber}x${padNumber(episodeNumber, 2)}`;
  const automaticSearchLabel = translate('AutomaticSearchForEpisode', {
    seriesTitle,
    episode,
    episodeTitle,
  });
  const interactiveSearchLabel = translate('InteractiveSearchForEpisode', {
    seriesTitle,
    episode,
    episodeTitle,
  });

  const handleSearchPress = useCallback(() => {
    executeCommand({
      name: CommandNames.EpisodeSearch,
      episodeIds: [episodeId],
    });
  }, [episodeId, executeCommand]);

  return (
    <Component className={className}>
      <SpinnerIconButton
        name={icons.SEARCH}
        isSpinning={isSearching}
        title={automaticSearchLabel}
        aria-label={automaticSearchLabel}
        onPress={handleSearchPress}
      />

      <IconButton
        name={icons.INTERACTIVE}
        title={interactiveSearchLabel}
        aria-label={interactiveSearchLabel}
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
        onModalClose={setDetailsModalClosed}
      />
    </Component>
  );
}

export default EpisodeSearchCell;
