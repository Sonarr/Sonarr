import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { align, icons } from 'Helpers/Props';
import hasDifferentItems from 'Utilities/Object/hasDifferentItems';
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
      <PageContent title="History">
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label="Refresh"
              iconName={icons.REFRESH}
              isSpinning={isFetching}
              onPress={onFirstPagePress}
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
            isFetchingAny && !isAllPopulated &&
              <LoadingIndicator />
          }

          {
            !isFetchingAny && hasError &&
              <div>Unable to load history</div>
          }

          {
            // If history isPopulated and it's empty show no history found and don't
            // wait for the episodes to populate because they are never coming.

            isPopulated && !hasError && !items.length &&
              <div>
                No history found
              </div>
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
        </PageContentBodyConnector>
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
  selectedFilterKey: PropTypes.string.isRequired,
  filters: PropTypes.arrayOf(PropTypes.object).isRequired,
  totalRecords: PropTypes.number,
  isEpisodesFetching: PropTypes.bool.isRequired,
  isEpisodesPopulated: PropTypes.bool.isRequired,
  episodesError: PropTypes.object,
  onFilterSelect: PropTypes.func.isRequired,
  onFirstPagePress: PropTypes.func.isRequired
};

export default History;
