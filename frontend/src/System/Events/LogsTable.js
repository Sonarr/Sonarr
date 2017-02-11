import PropTypes from 'prop-types';
import React, { Component } from 'react';
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
import FilterMenuItem from 'Components/Menu/FilterMenuItem';
import MenuContent from 'Components/Menu/MenuContent';
import LogsTableRow from './LogsTableRow';

class LogsTable extends Component {

  //
  // Listeners

  onFilterMenuItemPress = (filterKey, filterValue) => {
    this.props.onFilterSelect(filterKey, filterValue);
  }

  //
  // Render

  render() {
    const {
      isFetching,
      isPopulated,
      error,
      items,
      columns,
      filterKey,
      filterValue,
      totalRecords,
      clearLogExecuting,
      onRefreshPress,
      onClearLogsPress,
      ...otherProps
    } = this.props;

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
            <FilterMenu alignMenu={align.RIGHT}>
              <MenuContent>
                <FilterMenuItem
                  filterKey={filterKey}
                  filterValue={filterValue}
                  onPress={this.onFilterMenuItemPress}
                >
                  All
                </FilterMenuItem>

                <FilterMenuItem
                  name="level"
                  value="Info"
                  filterKey={filterKey}
                  filterValue={filterValue}
                  onPress={this.onFilterMenuItemPress}
                >
                  Info
                </FilterMenuItem>

                <FilterMenuItem
                  name="level"
                  value="Warn"
                  filterKey={filterKey}
                  filterValue={filterValue}
                  onPress={this.onFilterMenuItemPress}
                >
                  Warn
                </FilterMenuItem>

                <FilterMenuItem
                  name="level"
                  value="Error"
                  filterKey={filterKey}
                  filterValue={filterValue}
                  onPress={this.onFilterMenuItemPress}
                >
                  Error
                </FilterMenuItem>
              </MenuContent>
            </FilterMenu>
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

}

LogsTable.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  filterKey: PropTypes.string,
  filterValue: PropTypes.string,
  totalRecords: PropTypes.number,
  clearLogExecuting: PropTypes.bool.isRequired,
  onFilterSelect: PropTypes.func.isRequired,
  onRefreshPress: PropTypes.func.isRequired,
  onClearLogsPress: PropTypes.func.isRequired
};

export default LogsTable;
