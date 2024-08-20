import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { EPISODE_SEARCH } from 'Commands/commandNames';
import IconButton from 'Components/Link/IconButton';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import { EpisodeEntities } from 'Episode/useEpisode';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import { icons } from 'Helpers/Props';
import { executeCommand } from 'Store/Actions/commandActions';
import createExecutingCommandsSelector from 'Store/Selectors/createExecutingCommandsSelector';
import translate from 'Utilities/String/translate';
import EpisodeDetailsModal from './EpisodeDetailsModal';
import styles from './EpisodeSearchCell.css';

interface EpisodeSearchCellProps {
  episodeId: number;
  episodeEntity: EpisodeEntities;
  seriesId: number;
  episodeTitle: string;
}

function EpisodeSearchCell(props: EpisodeSearchCellProps) {
  const { episodeId, episodeEntity, seriesId, episodeTitle } = props;

  const executingCommands = useSelector(createExecutingCommandsSelector());
  const isSearching = executingCommands.some(({ name, body }) => {
    const { episodeIds = [] } = body;
    return name === EPISODE_SEARCH && episodeIds.indexOf(episodeId) > -1;
  });

  const dispatch = useDispatch();

  const [isDetailsModalOpen, setDetailsModalOpen, setDetailsModalClosed] =
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
    </TableRowCell>
  );
}

export default EpisodeSearchCell;
