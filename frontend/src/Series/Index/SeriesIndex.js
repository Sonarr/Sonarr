import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageJumpBar from 'Components/Page/PageJumpBar';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import TableOptionsModalWrapper from 'Components/Table/TableOptions/TableOptionsModalWrapper';
import { align, icons, sortDirections } from 'Helpers/Props';
import NoSeries from 'Series/NoSeries';
import hasDifferentItemsOrOrder from 'Utilities/Object/hasDifferentItemsOrOrder';
import SeriesIndexFilterMenu from './Menus/SeriesIndexFilterMenu';
import SeriesIndexSortMenu from './Menus/SeriesIndexSortMenu';
import SeriesIndexViewMenu from './Menus/SeriesIndexViewMenu';
import SeriesIndexOverviewOptionsModal from './Overview/Options/SeriesIndexOverviewOptionsModal';
import SeriesIndexOverviewsConnector from './Overview/SeriesIndexOverviewsConnector';
import SeriesIndexPosterOptionsModal from './Posters/Options/SeriesIndexPosterOptionsModal';
import SeriesIndexPostersConnector from './Posters/SeriesIndexPostersConnector';
import SeriesIndexFooterConnector from './SeriesIndexFooterConnector';
import SeriesIndexTableConnector from './Table/SeriesIndexTableConnector';
import SeriesIndexTableOptionsConnector from './Table/SeriesIndexTableOptionsConnector';
import styles from './SeriesIndex.css';

function getViewComponent(view) {
  if (view === 'posters') {
    return SeriesIndexPostersConnector;
  }

  if (view === 'overview') {
    return SeriesIndexOverviewsConnector;
  }

  return SeriesIndexTableConnector;
}

class SeriesIndex extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      scroller: null,
      jumpBarItems: { order: [] },
      jumpToCharacter: null,
      isPosterOptionsModalOpen: false,
      isOverviewOptionsModalOpen: false
    };
  }

  componentDidMount() {
    this.setJumpBarItems();
  }

  componentDidUpdate(prevProps) {
    const {
      items,
      sortKey,
      sortDirection
    } = this.props;

    if (sortKey !== prevProps.sortKey ||
        sortDirection !== prevProps.sortDirection ||
        hasDifferentItemsOrOrder(prevProps.items, items)
    ) {
      this.setJumpBarItems();
    }

    if (this.state.jumpToCharacter != null) {
      this.setState({ jumpToCharacter: null });
    }
  }

  //
  // Control

  setScrollerRef = (ref) => {
    this.setState({ scroller: ref });
  };

  setJumpBarItems() {
    const {
      items,
      sortKey,
      sortDirection
    } = this.props;

    // Reset if not sorting by sortTitle
    if (sortKey !== 'sortTitle') {
      this.setState({ jumpBarItems: { order: [] } });
      return;
    }

    const characters = _.reduce(items, (acc, item) => {
      let char = item.sortTitle.charAt(0);

      if (!isNaN(char)) {
        char = '#';
      }

      if (char in acc) {
        acc[char] = acc[char] + 1;
      } else {
        acc[char] = 1;
      }

      return acc;
    }, {});

    const order = Object.keys(characters).sort();

    // Reverse if sorting descending
    if (sortDirection === sortDirections.DESCENDING) {
      order.reverse();
    }

    const jumpBarItems = {
      characters,
      order
    };

    this.setState({ jumpBarItems });
  }

  //
  // Listeners

  onPosterOptionsPress = () => {
    this.setState({ isPosterOptionsModalOpen: true });
  };

  onPosterOptionsModalClose = () => {
    this.setState({ isPosterOptionsModalOpen: false });
  };

  onOverviewOptionsPress = () => {
    this.setState({ isOverviewOptionsModalOpen: true });
  };

  onOverviewOptionsModalClose = () => {
    this.setState({ isOverviewOptionsModalOpen: false });
  };

  onJumpBarItemPress = (jumpToCharacter) => {
    this.setState({ jumpToCharacter });
  };

  //
  // Render

  render() {
    const {
      isFetching,
      isPopulated,
      error,
      totalItems,
      items,
      columns,
      selectedFilterKey,
      filters,
      customFilters,
      sortKey,
      sortDirection,
      view,
      isRefreshingSeries,
      isRssSyncExecuting,
      onScroll,
      onSortSelect,
      onFilterSelect,
      onViewSelect,
      onRefreshSeriesPress,
      onRssSyncPress,
      ...otherProps
    } = this.props;

    const {
      scroller,
      jumpBarItems,
      jumpToCharacter,
      isPosterOptionsModalOpen,
      isOverviewOptionsModalOpen
    } = this.state;

    const ViewComponent = getViewComponent(view);
    const isLoaded = !!(!error && isPopulated && items.length && scroller);
    const hasNoSeries = !totalItems;

    return (
      <PageContent>
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label="Update all"
              iconName={icons.REFRESH}
              spinningName={icons.REFRESH}
              isSpinning={isRefreshingSeries}
              isDisabled={hasNoSeries}
              onPress={onRefreshSeriesPress}
            />

            <PageToolbarButton
              label="RSS Sync"
              iconName={icons.RSS}
              isSpinning={isRssSyncExecuting}
              isDisabled={hasNoSeries}
              onPress={onRssSyncPress}
            />

          </PageToolbarSection>

          <PageToolbarSection
            alignContent={align.RIGHT}
            collapseButtons={false}
          >
            {
              view === 'table' ?
                <TableOptionsModalWrapper
                  {...otherProps}
                  columns={columns}
                  optionsComponent={SeriesIndexTableOptionsConnector}
                >
                  <PageToolbarButton
                    label="Options"
                    iconName={icons.TABLE}
                  />
                </TableOptionsModalWrapper> :
                null
            }

            {
              view === 'posters' ?
                <PageToolbarButton
                  label="Options"
                  iconName={icons.POSTER}
                  isDisabled={hasNoSeries}
                  onPress={this.onPosterOptionsPress}
                /> :
                null
            }

            {
              view === 'overview' ?
                <PageToolbarButton
                  label="Options"
                  iconName={icons.OVERVIEW}
                  isDisabled={hasNoSeries}
                  onPress={this.onOverviewOptionsPress}
                /> :
                null
            }

            <PageToolbarSeparator />

            <SeriesIndexViewMenu
              view={view}
              isDisabled={hasNoSeries}
              onViewSelect={onViewSelect}
            />

            <SeriesIndexSortMenu
              sortKey={sortKey}
              sortDirection={sortDirection}
              isDisabled={hasNoSeries}
              onSortSelect={onSortSelect}
            />

            <SeriesIndexFilterMenu
              selectedFilterKey={selectedFilterKey}
              filters={filters}
              customFilters={customFilters}
              isDisabled={hasNoSeries}
              onFilterSelect={onFilterSelect}
            />
          </PageToolbarSection>
        </PageToolbar>

        <div className={styles.pageContentBodyWrapper}>
          <PageContentBody
            registerScroller={this.setScrollerRef}
            className={styles.contentBody}
            innerClassName={styles[`${view}InnerContentBody`]}
            onScroll={onScroll}
          >
            {
              isFetching && !isPopulated &&
                <LoadingIndicator />
            }

            {
              !isFetching && !!error &&
                <div>Unable to load series</div>
            }

            {
              isLoaded &&
                <div className={styles.contentBodyContainer}>
                  <ViewComponent
                    scroller={scroller}
                    items={items}
                    filters={filters}
                    sortKey={sortKey}
                    sortDirection={sortDirection}
                    jumpToCharacter={jumpToCharacter}
                    {...otherProps}
                  />

                  <SeriesIndexFooterConnector />
                </div>
            }

            {
              !error && isPopulated && !items.length &&
                <NoSeries totalItems={totalItems} />
            }
          </PageContentBody>

          {
            isLoaded && !!jumpBarItems.order.length &&
              <PageJumpBar
                items={jumpBarItems}
                onItemPress={this.onJumpBarItemPress}
              />
          }
        </div>

        <SeriesIndexPosterOptionsModal
          isOpen={isPosterOptionsModalOpen}
          onModalClose={this.onPosterOptionsModalClose}
        />

        <SeriesIndexOverviewOptionsModal
          isOpen={isOverviewOptionsModalOpen}
          onModalClose={this.onOverviewOptionsModalClose}
        />
      </PageContent>
    );
  }
}

SeriesIndex.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  totalItems: PropTypes.number.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  selectedFilterKey: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
  filters: PropTypes.arrayOf(PropTypes.object).isRequired,
  customFilters: PropTypes.arrayOf(PropTypes.object).isRequired,
  sortKey: PropTypes.string,
  sortDirection: PropTypes.oneOf(sortDirections.all),
  view: PropTypes.string.isRequired,
  isRefreshingSeries: PropTypes.bool.isRequired,
  isRssSyncExecuting: PropTypes.bool.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  onSortSelect: PropTypes.func.isRequired,
  onFilterSelect: PropTypes.func.isRequired,
  onViewSelect: PropTypes.func.isRequired,
  onRefreshSeriesPress: PropTypes.func.isRequired,
  onRssSyncPress: PropTypes.func.isRequired,
  onScroll: PropTypes.func.isRequired
};

export default SeriesIndex;
