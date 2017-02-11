import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import hasDifferentItems from 'Utilities/Object/hasDifferentItems';
import { align, icons, sortDirections } from 'Helpers/Props';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import PageJumpBar from 'Components/Page/PageJumpBar';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import NoSeries from 'Series/NoSeries';
import SeriesIndexTableConnector from './Table/SeriesIndexTableConnector';
import SeriesIndexPosterOptionsModal from './Posters/Options/SeriesIndexPosterOptionsModal';
import SeriesIndexPostersConnector from './Posters/SeriesIndexPostersConnector';
import SeriesIndexOverviewOptionsModal from './Overview/Options/SeriesIndexOverviewOptionsModal';
import SeriesIndexOverviewsConnector from './Overview/SeriesIndexOverviewsConnector';
import SeriesIndexFooter from './SeriesIndexFooter';
import SeriesIndexFilterMenu from './Menus/SeriesIndexFilterMenu';
import SeriesIndexSortMenu from './Menus/SeriesIndexSortMenu';
import SeriesIndexViewMenu from './Menus/SeriesIndexViewMenu';
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

    this._viewComponent = null;

    this.state = {
      contentBody: null,
      jumpBarItems: [],
      isPosterOptionsModalOpen: false,
      isOverviewOptionsModalOpen: false,
      isRendered: false
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

    if (
      hasDifferentItems(prevProps.items, items) ||
      sortKey !== prevProps.sortKey ||
      sortDirection !== prevProps.sortDirection
    ) {
      this.setJumpBarItems();
    }
  }

  //
  // Control

  setContentBodyRef = (ref) => {
    this.setState({ contentBody: ref });
  }

  setViewComponentRef = (ref) => {
    this._viewComponent = ref;
  }

  setJumpBarItems() {
    const {
      items,
      sortKey,
      sortDirection
    } = this.props;

    // Reset if not sorting by sortTitle
    if (sortKey !== 'sortTitle') {
      this.setState({ jumpBarItems: [] });
      return;
    }

    const characters = _.reduce(items, (acc, item) => {
      const firstCharacter = item.sortTitle.charAt(0);

      if (isNaN(firstCharacter)) {
        acc.push(firstCharacter);
      } else {
        acc.push('#');
      }

      return acc;
    }, []).sort();

    // Reverse if sorting descending
    if (sortDirection === sortDirections.DESCENDING) {
      characters.reverse();
    }

    this.setState({ jumpBarItems: _.sortedUniq(characters) });
  }

  //
  // Listeners

  onPosterOptionsPress = () => {
    this.setState({ isPosterOptionsModalOpen: true });
  }

  onPosterOptionsModalClose = () => {
    this.setState({ isPosterOptionsModalOpen: false });
  }

  onOverviewOptionsPress = () => {
    this.setState({ isOverviewOptionsModalOpen: true });
  }

  onOverviewOptionsModalClose = () => {
    this.setState({ isOverviewOptionsModalOpen: false });
  }

  onJumpBarItemPress = (item) => {
    const viewComponent = this._viewComponent.getWrappedInstance();
    viewComponent.scrollToFirstCharacter(item);
  }

  onRender = () => {
    this.setState({ isRendered: true }, () => {
      const {
        scrollTop,
        isSmallScreen
      } = this.props;

      if (isSmallScreen) {
        // Seems to result in the view being off by 125px (distance to the top of the page)
        // document.documentElement.scrollTop = document.body.scrollTop = scrollTop;

        // This works, but then jumps another 1px after scrolling
        document.documentElement.scrollTop = scrollTop;
      }
    });
  }

  onScroll = ({ scrollTop }) => {
    this.props.onScroll({ scrollTop });
  }

  //
  // Render

  render() {
    const {
      isFetching,
      isPopulated,
      error,
      items,
      filterKey,
      filterValue,
      sortKey,
      sortDirection,
      view,
      isRefreshingSeries,
      isRssSyncExecuting,
      scrollTop,
      onSortSelect,
      onFilterSelect,
      onViewSelect,
      onRefreshSeriesPress,
      onRssSyncPress,
      ...otherProps
    } = this.props;

    const {
      contentBody,
      jumpBarItems,
      isPosterOptionsModalOpen,
      isOverviewOptionsModalOpen,
      isRendered
    } = this.state;

    const ViewComponent = getViewComponent(view);
    const isLoaded = !error && isPopulated && !!items.length && contentBody;

    return (
      <PageContent>
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label="Update all"
              iconName={icons.REFRESH}
              spinningName={icons.REFRESH}
              isSpinning={isRefreshingSeries}
              onPress={onRefreshSeriesPress}
            />

            <PageToolbarButton
              label="RSS Sync"
              iconName={icons.RSS}
              isSpinning={isRssSyncExecuting}
              onPress={onRssSyncPress}
            />

          </PageToolbarSection>

          <PageToolbarSection
            alignContent={align.RIGHT}
            collapseButtons={false}
          >

            {
              view === 'posters' &&
                <PageToolbarButton
                  label="Options"
                  iconName={icons.POSTER}
                  onPress={this.onPosterOptionsPress}
                />
            }

            {
              view === 'overview' &&
                <PageToolbarButton
                  label="Options"
                  iconName={icons.OVERVIEW}
                  onPress={this.onOverviewOptionsPress}
                />
            }

            {
              (view === 'posters' || view === 'overview') &&
                <PageToolbarSeparator />
            }

            <SeriesIndexViewMenu
              view={view}
              onViewSelect={onViewSelect}
            />

            <SeriesIndexSortMenu
              sortKey={sortKey}
              sortDirection={sortDirection}
              onSortSelect={onSortSelect}
            />

            <SeriesIndexFilterMenu
              filterKey={filterKey}
              filterValue={filterValue}
              onFilterSelect={onFilterSelect}
            />
          </PageToolbarSection>
        </PageToolbar>

        <div className={styles.pageContentBodyWrapper}>
          <PageContentBodyConnector
            ref={this.setContentBodyRef}
            className={styles.contentBody}
            innerClassName={styles[`${view}InnerContentBody`]}
            scrollTop={isRendered ? scrollTop : 0}
            onScroll={this.onScroll}
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
                    ref={this.setViewComponentRef}
                    contentBody={contentBody}
                    scrollTop={scrollTop}
                    onRender={this.onRender}
                    {...otherProps}
                  />

                  <SeriesIndexFooter
                    series={items}
                  />
                </div>
            }

            {
              !error && isPopulated && !items.length &&
                <NoSeries />
            }
          </PageContentBodyConnector>

          {
            isLoaded && !!jumpBarItems.length &&
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
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  filterKey: PropTypes.string,
  filterValue: PropTypes.oneOfType([PropTypes.bool, PropTypes.number, PropTypes.string]),
  sortKey: PropTypes.string,
  sortDirection: PropTypes.oneOf(sortDirections.all),
  view: PropTypes.string.isRequired,
  isRefreshingSeries: PropTypes.bool.isRequired,
  isRssSyncExecuting: PropTypes.bool.isRequired,
  scrollTop: PropTypes.number.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  onSortSelect: PropTypes.func.isRequired,
  onFilterSelect: PropTypes.func.isRequired,
  onViewSelect: PropTypes.func.isRequired,
  onRefreshSeriesPress: PropTypes.func.isRequired,
  onRssSyncPress: PropTypes.func.isRequired,
  onScroll: PropTypes.func.isRequired
};

export default SeriesIndex;
