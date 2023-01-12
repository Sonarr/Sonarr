import classNames from 'classnames';
import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import { SelectActionType, useSelect } from 'App/SelectContext';
import IconButton from 'Components/Link/IconButton';
import Column from 'Components/Table/Column';
import TableOptionsModalWrapper from 'Components/Table/TableOptions/TableOptionsModalWrapper';
import VirtualTableHeader from 'Components/Table/VirtualTableHeader';
import VirtualTableHeaderCell from 'Components/Table/VirtualTableHeaderCell';
import VirtualTableSelectAllHeaderCell from 'Components/Table/VirtualTableSelectAllHeaderCell';
import { icons } from 'Helpers/Props';
import SortDirection from 'Helpers/Props/SortDirection';
import {
  setSeriesSort,
  setSeriesTableOption,
} from 'Store/Actions/seriesIndexActions';
import hasGrowableColumns from './hasGrowableColumns';
import SeriesIndexTableOptions from './SeriesIndexTableOptions';
import styles from './SeriesIndexTableHeader.css';

interface SeriesIndexTableHeaderProps {
  showBanners: boolean;
  columns: Column[];
  sortKey?: string;
  sortDirection?: SortDirection;
  isSelectMode: boolean;
}

function SeriesIndexTableHeader(props: SeriesIndexTableHeaderProps) {
  const { showBanners, columns, sortKey, sortDirection, isSelectMode } = props;
  const dispatch = useDispatch();
  const [selectState, selectDispatch] = useSelect();

  const onSortPress = useCallback(
    (value) => {
      dispatch(setSeriesSort({ sortKey: value }));
    },
    [dispatch]
  );

  const onTableOptionChange = useCallback(
    (payload) => {
      dispatch(setSeriesTableOption(payload));
    },
    [dispatch]
  );

  const onSelectAllChange = useCallback(
    ({ value }) => {
      selectDispatch({
        type: value ? SelectActionType.SelectAll : SelectActionType.UnselectAll,
      });
    },
    [selectDispatch]
  );

  return (
    <VirtualTableHeader>
      {isSelectMode ? (
        <VirtualTableSelectAllHeaderCell
          allSelected={selectState.allSelected}
          allUnselected={selectState.allUnselected}
          onSelectAllChange={onSelectAllChange}
        />
      ) : null}

      {columns.map((column) => {
        const { name, label, isSortable, isVisible } = column;

        if (!isVisible) {
          return null;
        }

        if (name === 'actions') {
          return (
            <VirtualTableHeaderCell
              key={name}
              className={styles[name]}
              name={name}
              isSortable={false}
            >
              <TableOptionsModalWrapper
                columns={columns}
                optionsComponent={SeriesIndexTableOptions}
                onTableOptionChange={onTableOptionChange}
              >
                <IconButton name={icons.ADVANCED_SETTINGS} />
              </TableOptionsModalWrapper>
            </VirtualTableHeaderCell>
          );
        }

        return (
          <VirtualTableHeaderCell
            key={name}
            className={classNames(
              styles[name],
              name === 'sortTitle' && showBanners && styles.banner,
              name === 'sortTitle' &&
                showBanners &&
                !hasGrowableColumns(columns) &&
                styles.bannerGrow
            )}
            name={name}
            sortKey={sortKey}
            sortDirection={sortDirection}
            isSortable={isSortable}
            onSortPress={onSortPress}
          >
            {label}
          </VirtualTableHeaderCell>
        );
      })}
    </VirtualTableHeader>
  );
}

export default SeriesIndexTableHeader;
