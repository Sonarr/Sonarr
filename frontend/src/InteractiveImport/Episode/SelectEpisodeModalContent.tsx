import React, { useCallback, useMemo, useState } from 'react';
import { SelectProvider, useSelect } from 'App/Select/SelectContext';
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
import {
  setEpisodeSelectionSort,
  useEpisodeSelectionOptions,
} from 'Episode/episodeSelectionOptionsStore';
import useEpisodes from 'Episode/useEpisodes';
import { kinds, scrollDirections } from 'Helpers/Props';
import { SortDirection } from 'Helpers/Props/sortDirections';
import { CheckInputChanged, InputChanged } from 'typings/inputs';
import clientSideFilterAndSort from 'Utilities/Filter/clientSideFilterAndSort';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import translate from 'Utilities/String/translate';
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
    label: () => translate('Title'),
    isVisible: true,
  },
  {
    name: 'airDate',
    label: () => translate('AirDate'),
    isVisible: true,
  },
];

export interface SelectedEpisode {
  id: number;
  episodes: Episode[];
}

interface SelectEpisodeModalContentProps {
  selectedIds: number[] | string[];
  seriesId?: number;
  seasonNumber?: number;
  selectedDetails?: string;
  isAnime: boolean;
  modalTitle: string;
  onEpisodesSelect(selectedEpisodes: SelectedEpisode[]): unknown;
  onModalClose(): unknown;
}

function SelectEpisodeModalContentInner(props: SelectEpisodeModalContentProps) {
  const {
    selectedIds,
    seriesId,
    seasonNumber,
    selectedDetails,
    isAnime,
    modalTitle,
    onEpisodesSelect,
    onModalClose,
  } = props;

  const [filter, setFilter] = useState('');

  const { isFetching, isFetched, data, error } = useEpisodes({
    seriesId,
    seasonNumber,
  });

  const { sortKey, sortDirection } = useEpisodeSelectionOptions();

  const {
    allSelected,
    allUnselected,
    selectedCount: selectedEpisodesCount,
    getSelectedIds,
    selectAll,
    unselectAll,
  } = useSelect<Episode>();

  const filterEpisodeNumber = parseInt(filter);
  const errorMessage = getErrorMessage(error, translate('EpisodesLoadError'));
  const selectedCount = selectedIds.length;
  const selectionIsValid =
    selectedEpisodesCount > 0 && selectedEpisodesCount % selectedCount === 0;

  const onFilterChange = useCallback(
    ({ value }: InputChanged<string>) => {
      setFilter(value.toLowerCase());
    },
    [setFilter]
  );

  const onSelectAllChange = useCallback(
    ({ value }: CheckInputChanged) => {
      if (value) {
        selectAll();
      } else {
        unselectAll();
      }
    },
    [selectAll, unselectAll]
  );

  const onSortPress = useCallback(
    (newSortKey: string, newSortDirection?: SortDirection) => {
      setEpisodeSelectionSort({
        sortKey: newSortKey,
        sortDirection: newSortDirection,
      });
    },
    []
  );

  const onEpisodesSelectWrapper = useCallback(() => {
    const episodeIds: number[] = getSelectedIds();

    const selectedEpisodes = data.reduce((acc: Episode[], item) => {
      if (episodeIds.indexOf(item.id) > -1) {
        acc.push(item);
      }

      return acc;
    }, []);

    const episodesPerFile = selectedEpisodes.length / selectedIds.length;
    const sortedEpisodes = selectedEpisodes.sort((a, b) => {
      return a.seasonNumber - b.seasonNumber;
    });

    const mappedEpisodes = selectedIds.map((id, index): SelectedEpisode => {
      const startingIndex = index * episodesPerFile;
      const episodes = sortedEpisodes.slice(
        startingIndex,
        startingIndex + episodesPerFile
      );

      return {
        id: id as number,
        episodes,
      };
    });

    onEpisodesSelect(mappedEpisodes);
  }, [selectedIds, data, getSelectedIds, onEpisodesSelect]);

  let details = selectedDetails;

  if (!details) {
    details =
      selectedCount > 1
        ? translate('CountSelectedFiles', { selectedCount })
        : translate('CountSelectedFile', { selectedCount });
  }

  const { data: items } = useMemo(() => {
    return clientSideFilterAndSort<Episode>(data, {
      sortKey,
      sortDirection,
    });
  }, [data, sortKey, sortDirection]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {translate('SelectEpisodesModalTitle', { modalTitle })}
      </ModalHeader>

      <ModalBody
        className={styles.modalBody}
        scrollDirection={scrollDirections.NONE}
      >
        <TextInput
          className={styles.filterInput}
          placeholder={translate('FilterEpisodesPlaceholder')}
          name="filter"
          value={filter}
          autoFocus={true}
          onChange={onFilterChange}
        />

        <Scroller className={styles.scroller} autoFocus={false}>
          {isFetching ? <LoadingIndicator /> : null}

          {error ? <div>{errorMessage}</div> : null}

          {isFetched && !!items.length ? (
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
                    />
                  ) : null;
                })}
              </TableBody>
            </Table>
          ) : null}

          {isFetched && !data.length
            ? translate('NoEpisodesFoundForSelectedSeason')
            : null}
        </Scroller>
      </ModalBody>

      <ModalFooter className={styles.footer}>
        <div className={styles.details}>{details}</div>

        <div className={styles.buttons}>
          <Button onPress={onModalClose}>{translate('Cancel')}</Button>

          <Button
            kind={kinds.SUCCESS}
            isDisabled={!selectionIsValid}
            onPress={onEpisodesSelectWrapper}
          >
            {translate('SelectEpisodes')}
          </Button>
        </div>
      </ModalFooter>
    </ModalContent>
  );
}

function SelectEpisodeModalContent(props: SelectEpisodeModalContentProps) {
  const { data } = useEpisodes({
    seriesId: props.seriesId,
    seasonNumber: props.seasonNumber,
  });

  return (
    <SelectProvider items={data}>
      <SelectEpisodeModalContentInner {...props} />
    </SelectProvider>
  );
}

export default SelectEpisodeModalContent;
