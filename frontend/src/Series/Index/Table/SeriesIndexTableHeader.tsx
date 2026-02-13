import classNames from 'classnames';
import React, { useCallback } from 'react';
import { useSelect } from 'App/Select/SelectContext';
import IconButton from 'Components/Link/IconButton';
import Column from 'Components/Table/Column';
import TableOptionsModalWrapper from 'Components/Table/TableOptions/TableOptionsModalWrapper';
import VirtualTableHeader from 'Components/Table/VirtualTableHeader';
import VirtualTableHeaderCell from 'Components/Table/VirtualTableHeaderCell';
import VirtualTableSelectAllHeaderCell from 'Components/Table/VirtualTableSelectAllHeaderCell';
import { icons } from 'Helpers/Props';
import { SortDirection } from 'Helpers/Props/sortDirections';
import {
  setSeriesOption,
  setSeriesSort,
  setSeriesTableOptions,
} from 'Series/seriesOptionsStore';
import { CheckInputChanged } from 'typings/inputs';
import { TableOptionsChangePayload } from 'typings/Table';
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
  const { allSelected, allUnselected, selectAll, unselectAll } = useSelect();

  const onSortPress = useCallback(
    (sortKey: string, sortDirection?: SortDirection) => {
      setSeriesSort({ sortKey, sortDirection });
    },
    []
  );

  const onTableOptionChange = useCallback(
    (
      payload: TableOptionsChangePayload & {
        tableOptions?: { showBanners?: boolean; showSearchAction?: boolean };
      }
    ) => {
      if (payload.tableOptions) {
        setSeriesTableOptions(payload.tableOptions);
      } else if (payload.columns) {
        setSeriesOption('columns', payload.columns);
      }
    },
    []
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

  return (
    <VirtualTableHeader>
      {isSelectMode ? (
        <VirtualTableSelectAllHeaderCell
          allSelected={allSelected}
          allUnselected={allUnselected}
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
              // eslint-disable-next-line @typescript-eslint/ban-ts-comment
              // @ts-ignore
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
            {typeof label === 'function' ? label() : label}
          </VirtualTableHeaderCell>
        );
      })}
    </VirtualTableHeader>
  );
}

export default SeriesIndexTableHeader;
