import React, { useCallback, useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import TextInput from 'Components/Form/TextInput';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Scroller from 'Components/Scroller/Scroller';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import Episode from 'Episode/Episode';
import useSelectState from 'Helpers/Hooks/useSelectState';
import { kinds, scrollDirections } from 'Helpers/Props';
import {
  clearEpisodes,
  fetchEpisodes,
  setEpisodesSort,
} from 'Store/Actions/episodeSelectionActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import SelectEpisodeRow from './SelectEpisodeRow';
import styles from './SelectEpisodeModalContent.css';

const columns = [
  {
    name: 'episodeNumber',
    label: '#',
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'title',
    label: 'Title',
    isVisible: true,
  },
  {
    name: 'airDate',
    label: 'Air Date',
    isVisible: true,
  },
];

function episodesSelector() {
  return createSelector(
    createClientSideCollectionSelector('episodeSelection'),
    (episodes) => {
      return episodes;
    }
  );
}

export interface SelectedEpisode {
  fileId: number;
  episodes: Episode[];
}

interface SelectEpisodeModalContentProps {
  selectedIds: number[] | string[];
  seriesId: number;
  seasonNumber: number;
  selectedDetails?: string;
  isAnime: boolean;
  sortKey?: string;
  sortDirection?: string;
  modalTitle?: string;
  onEpisodesSelect(selectedEpisodes: SelectedEpisode[]): unknown;
  onModalClose(): unknown;
}

//
// Render

function SelectEpisodeModalContent(props: SelectEpisodeModalContentProps) {
  const {
    selectedIds,
    seriesId,
    seasonNumber,
    selectedDetails,
    isAnime,
    sortKey,
    sortDirection,
    modalTitle,
    onEpisodesSelect,
    onModalClose,
  } = props;

  const [filter, setFilter] = useState('');
  const [selectState, setSelectState] = useSelectState();

  const { allSelected, allUnselected, selectedState } = selectState;
  const { isFetching, isPopulated, items, error } = useSelector(
    episodesSelector()
  );
  const dispatch = useDispatch();

  const filterEpisodeNumber = parseInt(filter);
  const errorMessage = getErrorMessage(error, 'Unable to load episodes');
  const selectedCount = selectedIds.length;
  const selectedEpisodesCount = getSelectedIds(selectState).length;
  const selectionIsValid =
    selectedEpisodesCount > 0 && selectedEpisodesCount % selectedCount === 0;

  const onFilterChange = useCallback(
    ({ value }) => {
      setFilter(value.toLowerCase());
    },
    [setFilter]
  );

  const onSelectAllChange = useCallback(
    ({ value }) => {
      setSelectState({ type: value ? 'selectAll' : 'unselectAll', items });
    },
    [items, setSelectState]
  );

  const onSelectedChange = useCallback(
    ({ id, value, shiftKey = false }) => {
      setSelectState({
        type: 'toggleSelected',
        items,
        id,
        isSelected: value,
        shiftKey,
      });
    },
    [items, setSelectState]
  );

  const onSortPress = useCallback(
    (newSortKey, newSortDirection) => {
      dispatch(
        setEpisodesSort({
          sortKey: newSortKey,
          sortDirection: newSortDirection,
        })
      );
    },
    [dispatch]
  );

  const onEpisodesSelectWrapper = useCallback(() => {
    const episodeIds = getSelectedIds(selectedState);

    const selectedEpisodes = items.reduce((acc, item) => {
      if (episodeIds.indexOf(item.id) > -1) {
        acc.push(item);
      }

      return acc;
    }, []);

    const episodesPerFile = selectedEpisodes.length / selectedIds.length;
    const sortedEpisodes = selectedEpisodes.sort((a, b) => {
      return a.seasonNumber - b.seasonNumber;
    });

    const mappedEpisodes = selectedIds.map((fileId, index): SelectedEpisode => {
      const startingIndex = index * episodesPerFile;
      const episodes = sortedEpisodes.slice(
        startingIndex,
        startingIndex + episodesPerFile
      );

      return {
        fileId,
        episodes,
      };
    });

    onEpisodesSelect(mappedEpisodes);
  }, [selectedIds, items, selectedState, onEpisodesSelect]);

  useEffect(
    () => {
      dispatch(fetchEpisodes({ seriesId, seasonNumber }));

      return () => {
        dispatch(clearEpisodes());
      };
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    []
  );

  let details = selectedDetails;

  if (!details) {
    details =
      selectedCount > 1
        ? `${selectedCount} selected files`
        : `${selectedCount} selected file`;
  }

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{modalTitle} - Select Episode(s)</ModalHeader>

      <ModalBody
        className={styles.modalBody}
        scrollDirection={scrollDirections.NONE}
      >
        <TextInput
          className={styles.filterInput}
          placeholder="Filter episodes by title or number"
          name="filter"
          value={filter}
          autoFocus={true}
          onChange={onFilterChange}
        />

        <Scroller className={styles.scroller} autoFocus={false}>
          {isFetching ? <LoadingIndicator /> : null}

          {error ? <div>{errorMessage}</div> : null}

          {isPopulated && !!items.length ? (
            <Table
              columns={columns}
              selectAll={true}
              allSelected={allSelected}
              allUnselected={allUnselected}
              sortKey={sortKey}
              sortDirection={sortDirection}
              onSortPress={onSortPress}
              onSelectAllChange={onSelectAllChange}
            >
              <TableBody>
                {items.map((item) => {
                  return item.title.toLowerCase().includes(filter) ||
                    item.episodeNumber === filterEpisodeNumber ? (
                    <SelectEpisodeRow
                      key={item.id}
                      id={item.id}
                      episodeNumber={item.episodeNumber}
                      absoluteEpisodeNumber={item.absoluteEpisodeNumber}
                      title={item.title}
                      airDate={item.airDate}
                      isAnime={isAnime}
                      isSelected={selectedState[item.id]}
                      onSelectedChange={onSelectedChange}
                    />
                  ) : null;
                })}
              </TableBody>
            </Table>
          ) : null}

          {isPopulated && !items.length
            ? 'No episodes were found for the selected season'
            : null}
        </Scroller>
      </ModalBody>

      <ModalFooter className={styles.footer}>
        <div className={styles.details}>{details}</div>

        <div className={styles.buttons}>
          <Button onPress={onModalClose}>Cancel</Button>

          <Button
            kind={kinds.SUCCESS}
            isDisabled={!selectionIsValid}
            onPress={onEpisodesSelectWrapper}
          >
            Select Episodes
          </Button>
        </div>
      </ModalFooter>
    </ModalContent>
  );
}

export default SelectEpisodeModalContent;
