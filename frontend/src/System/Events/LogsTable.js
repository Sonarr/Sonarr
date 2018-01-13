import PropTypes from 'prop-types';
import React from 'react';
import { align, icons } from 'Helpers/Props';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import TablePager from 'Components/Table/TablePager';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import FilterMenu from 'Components/Menu/FilterMenu';
import LogsTableRow from './LogsTableRow';

function LogsTable(props) {
  const {
    isFetching,
    isPopulated,
    error,
    items,
    columns,
    selectedFilterKey,
    filters,
    totalRecords,
    clearLogExecuting,
    onRefreshPress,
    onClearLogsPress,
    onFilterSelect,
    ...otherProps
  } = props;

  return (
    <PageContent title="Logs">
      <PageToolbar>
        <PageToolbarSection>
          <PageToolbarButton
            label="Refresh"
            iconName={icons.REFRESH}
            spinningName={icons.REFRESH}
            isSpinning={isFetching}
            onPress={onRefreshPress}
          />

          <PageToolbarButton
            label="Clear"
            iconName={icons.CLEAR}
            isSpinning={clearLogExecuting}
            onPress={onClearLogsPress}
          />
        </PageToolbarSection>

        <PageToolbarSection alignContent={align.RIGHT}>
          <FilterMenu
            alignMenu={align.RIGHT}
            selectedFilterKey={selectedFilterKey}
            filters={filters}
            customFilters={[]}
            onFilterSelect={onFilterSelect}
          />
        </PageToolbarSection>
      </PageToolbar>

      <PageContentBodyConnector>
        {
          isFetching && !isPopulated &&
          <LoadingIndicator />
        }

        {
          isPopulated && !error && !items.length &&
          <div>
                No logs found
          </div>
        }

        {
          isPopulated && !error && !!items.length &&
          <div>
            <Table
              columns={columns}
              canModifyColumns={false}
              {...otherProps}
            >
              <TableBody>
                {
                  items.map((item) => {
                    return (
                      <LogsTableRow
                        key={item.id}
                        columns={columns}
                        {...item}
                      />
                    );
                  })
                }
              </TableBody>
            </Table>

            <TablePager
              totalRecords={totalRecords}
              isFetching={isFetching}
              {...otherProps}
            />
          </div>
        }
      </PageContentBodyConnector>
    </PageContent>
  );
}

LogsTable.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  selectedFilterKey: PropTypes.string.isRequired,
  filters: PropTypes.arrayOf(PropTypes.object).isRequired,
  totalRecords: PropTypes.number,
  clearLogExecuting: PropTypes.bool.isRequired,
  onFilterSelect: PropTypes.func.isRequired,
  onRefreshPress: PropTypes.func.isRequired,
  onClearLogsPress: PropTypes.func.isRequired
};

export default LogsTable;
