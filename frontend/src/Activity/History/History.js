import PropTypes from 'prop-types';
import React, { Component } from 'react';
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
import hasDifferentItems from 'Utilities/Object/hasDifferentItems';
import translate from 'Utilities/String/translate';
import HistoryFilterModal from './HistoryFilterModal';
import HistoryRowConnector from './HistoryRowConnector';

class History extends Component {

  //
  // Lifecycle

  shouldComponentUpdate(nextProps) {
    // Don't update when fetching has completed if items have changed,
    // before episodes start fetching or when episodes start fetching.

    if (
      (
        this.props.isFetching &&
        nextProps.isPopulated &&
        hasDifferentItems(this.props.items, nextProps.items)
      ) ||
      (!this.props.isEpisodesFetching && nextProps.isEpisodesFetching)
    ) {
      return false;
    }

    return true;
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
      selectedFilterKey,
      filters,
      customFilters,
      totalRecords,
      isEpisodesFetching,
      isEpisodesPopulated,
      episodesError,
      onFilterSelect,
      onFirstPagePress,
      ...otherProps
    } = this.props;

    const isFetchingAny = isFetching || isEpisodesFetching;
    const isAllPopulated = isPopulated && (isEpisodesPopulated || !items.length);
    const hasError = error || episodesError;

    return (
      <PageContent title={translate('History')}>
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label={translate('Refresh')}
              iconName={icons.REFRESH}
              isSpinning={isFetching}
              onPress={onFirstPagePress}
            />
          </PageToolbarSection>

          <PageToolbarSection alignContent={align.RIGHT}>
            <TableOptionsModalWrapper
              {...otherProps}
              columns={columns}
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
              customFilters={customFilters}
              filterModalConnectorComponent={HistoryFilterModal}
              onFilterSelect={onFilterSelect}
            />
          </PageToolbarSection>
        </PageToolbar>

        <PageContentBody>
          {
            isFetchingAny && !isAllPopulated &&
              <LoadingIndicator />
          }

          {
            !isFetchingAny && hasError &&
              <Alert kind={kinds.DANGER}>
                {translate('HistoryLoadError')}
              </Alert>
          }

          {
            // If history isPopulated and it's empty show no history found and don't
            // wait for the episodes to populate because they are never coming.

            isPopulated && !hasError && !items.length &&
              <Alert kind={kinds.INFO}>
                {translate('NoHistoryFound')}
              </Alert>
          }

          {
            isAllPopulated && !hasError && !!items.length &&
              <div>
                <Table
                  columns={columns}
                  {...otherProps}
                >
                  <TableBody>
                    {
                      items.map((item) => {
                        return (
                          <HistoryRowConnector
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
                  isFetching={isFetchingAny}
                  onFirstPagePress={onFirstPagePress}
                  {...otherProps}
                />
              </div>
          }
        </PageContentBody>
      </PageContent>
    );
  }
}

History.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  selectedFilterKey: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
  filters: PropTypes.arrayOf(PropTypes.object).isRequired,
  customFilters: PropTypes.arrayOf(PropTypes.object).isRequired,
  totalRecords: PropTypes.number,
  isEpisodesFetching: PropTypes.bool.isRequired,
  isEpisodesPopulated: PropTypes.bool.isRequired,
  episodesError: PropTypes.object,
  onFilterSelect: PropTypes.func.isRequired,
  onFirstPagePress: PropTypes.func.isRequired
};

export default History;
