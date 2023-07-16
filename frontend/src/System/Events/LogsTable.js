import PropTypes from 'prop-types';
import React from 'react';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import FilterMenu from 'Components/Menu/FilterMenu';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import TableOptionsModalWrapper from 'Components/Table/TableOptions/TableOptionsModalWrapper';
import TablePager from 'Components/Table/TablePager';
import { align, icons, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
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
    <PageContent title={translate('Logs')}>
      <PageToolbar>
        <PageToolbarSection>
          <PageToolbarButton
            label={translate('Refresh')}
            iconName={icons.REFRESH}
            spinningName={icons.REFRESH}
            isSpinning={isFetching}
            onPress={onRefreshPress}
          />

          <PageToolbarButton
            label={translate('Clear')}
            iconName={icons.CLEAR}
            isSpinning={clearLogExecuting}
            onPress={onClearLogsPress}
          />
        </PageToolbarSection>

        <PageToolbarSection alignContent={align.RIGHT}>
          <TableOptionsModalWrapper
            {...otherProps}
            columns={columns}
            canModifyColumns={false}
          >
            <PageToolbarButton
              label={translate('Options')}
              iconName={icons.TABLE}
            />
          </TableOptionsModalWrapper>

          <FilterMenu
            alignMenu={align.RIGHT}
            selectedFilterKey={selectedFilterKey}
            filters={filters}
            customFilters={[]}
            onFilterSelect={onFilterSelect}
          />
        </PageToolbarSection>
      </PageToolbar>

      <PageContentBody>
        {
          isFetching && !isPopulated &&
            <LoadingIndicator />
        }

        {
          isPopulated && !error && !items.length &&
            <Alert kind={kinds.INFO}>
              {translate('NoEventsFound')}
            </Alert>
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
      </PageContentBody>
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
