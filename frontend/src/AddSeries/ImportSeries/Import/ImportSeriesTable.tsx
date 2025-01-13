import React, { RefObject, useCallback, useEffect, useRef } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { FixedSizeList, ListChildComponentProps } from 'react-window';
import { useSelect } from 'App/SelectContext';
import AppState from 'App/State/AppState';
import { ImportSeries } from 'App/State/ImportSeriesAppState';
import VirtualTable from 'Components/Table/VirtualTable';
import usePrevious from 'Helpers/Hooks/usePrevious';
import {
  queueLookupSeries,
  setImportSeriesValue,
} from 'Store/Actions/importSeriesActions';
import createAllSeriesSelector from 'Store/Selectors/createAllSeriesSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import { CheckInputChanged } from 'typings/inputs';
import { SelectStateInputProps } from 'typings/props';
import { UnmappedFolder } from 'typings/RootFolder';
import ImportSeriesHeader from './ImportSeriesHeader';
import ImportSeriesRow from './ImportSeriesRow';
import styles from './ImportSeriesTable.css';

const ROW_HEIGHT = 52;

interface RowItemData {
  items: ImportSeries[];
}

interface ImportSeriesTableProps {
  unmappedFolders: UnmappedFolder[];
  scrollerRef: RefObject<HTMLElement>;
}

function Row({ index, style, data }: ListChildComponentProps<RowItemData>) {
  const { items } = data;

  if (index >= items.length) {
    return null;
  }

  const item = items[index];

  return (
    <div
      style={{
        display: 'flex',
        justifyContent: 'space-between',
        ...style,
      }}
      className={styles.row}
    >
      <ImportSeriesRow key={item.id} id={item.id} />
    </div>
  );
}

function ImportSeriesTable({
  unmappedFolders,
  scrollerRef,
}: ImportSeriesTableProps) {
  const dispatch = useDispatch();

  const { monitor, qualityProfileId, seriesType, seasonFolder } = useSelector(
    (state: AppState) => state.addSeries.defaults
  );

  const items = useSelector((state: AppState) => state.importSeries.items);
  const { isSmallScreen } = useSelector(createDimensionsSelector());
  const allSeries = useSelector(createAllSeriesSelector());
  const [selectState, selectDispatch] = useSelect();

  const defaultValues = useRef({
    monitor,
    qualityProfileId,
    seriesType,
    seasonFolder,
  });

  const listRef = useRef<FixedSizeList<RowItemData>>(null);
  const initialUnmappedFolders = useRef(unmappedFolders);
  const previousItems = usePrevious(items);
  const { allSelected, allUnselected, selectedState } = selectState;

  const handleSelectAllChange = useCallback(
    ({ value }: CheckInputChanged) => {
      selectDispatch({
        type: value ? 'selectAll' : 'unselectAll',
      });
    },
    [selectDispatch]
  );

  const handleSelectedChange = useCallback(
    ({ id, value, shiftKey }: SelectStateInputProps) => {
      selectDispatch({
        type: 'toggleSelected',
        id,
        isSelected: value,
        shiftKey,
      });
    },
    [selectDispatch]
  );

  const handleRemoveSelectedStateItem = useCallback(
    (id: string) => {
      selectDispatch({
        type: 'removeItem',
        id,
      });
    },
    [selectDispatch]
  );

  useEffect(() => {
    initialUnmappedFolders.current.forEach(({ name, path, relativePath }) => {
      dispatch(
        queueLookupSeries({
          name,
          path,
          relativePath,
          term: name,
        })
      );

      dispatch(
        // @ts-expect-error - actions are not typed
        setImportSeriesValue({
          id: name,
          ...defaultValues.current,
        })
      );
    });
  }, [dispatch]);

  useEffect(() => {
    previousItems?.forEach((prevItem) => {
      const { id } = prevItem;

      const item = items.find((i) => i.id === id);

      if (!item) {
        handleRemoveSelectedStateItem(id);
        return;
      }

      const selectedSeries = item.selectedSeries;
      const isSelected = selectedState[id];

      const isExistingSeries =
        !!selectedSeries &&
        allSeries.some((s) => s.tvdbId === selectedSeries.tvdbId);

      if (
        (!selectedSeries && prevItem.selectedSeries) ||
        (isExistingSeries && !prevItem.selectedSeries)
      ) {
        handleSelectedChange({ id, value: false, shiftKey: false });

        return;
      }

      if (isSelected && (!selectedSeries || isExistingSeries)) {
        handleSelectedChange({ id, value: false, shiftKey: false });

        return;
      }

      if (selectedSeries && selectedSeries !== prevItem.selectedSeries) {
        handleSelectedChange({ id, value: true, shiftKey: false });

        return;
      }
    });
  }, [
    allSeries,
    items,
    previousItems,
    selectedState,
    handleRemoveSelectedStateItem,
    handleSelectedChange,
  ]);

  if (!items.length) {
    return null;
  }

  return (
    <VirtualTable
      Header={
        <ImportSeriesHeader
          allSelected={allSelected}
          allUnselected={allUnselected}
          onSelectAllChange={handleSelectAllChange}
        />
      }
      itemCount={items.length}
      itemData={{
        items,
      }}
      isSmallScreen={isSmallScreen}
      listRef={listRef}
      rowHeight={ROW_HEIGHT}
      Row={Row}
      scrollerRef={scrollerRef}
    />
  );
}

export default ImportSeriesTable;
