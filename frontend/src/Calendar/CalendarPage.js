import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Measure from 'Components/Measure';
import FilterMenu from 'Components/Menu/FilterMenu';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import { align, icons } from 'Helpers/Props';
import NoSeries from 'Series/NoSeries';
import CalendarConnector from './CalendarConnector';
import CalendarLinkModal from './iCal/CalendarLinkModal';
import LegendConnector from './Legend/LegendConnector';
import CalendarOptionsModal from './Options/CalendarOptionsModal';
import styles from './CalendarPage.css';

const MINIMUM_DAY_WIDTH = 120;

class CalendarPage extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isCalendarLinkModalOpen: false,
      isOptionsModalOpen: false,
      width: 0
    };
  }

  //
  // Listeners

  onMeasure = ({ width }) => {
    this.setState({ width });
    const days = Math.max(3, Math.min(7, Math.floor(width / MINIMUM_DAY_WIDTH)));

    this.props.onDaysCountChange(days);
  };

  onGetCalendarLinkPress = () => {
    this.setState({ isCalendarLinkModalOpen: true });
  };

  onGetCalendarLinkModalClose = () => {
    this.setState({ isCalendarLinkModalOpen: false });
  };

  onOptionsPress = () => {
    this.setState({ isOptionsModalOpen: true });
  };

  onOptionsModalClose = () => {
    this.setState({ isOptionsModalOpen: false });
  };

  onSearchMissingPress = () => {
    const {
      missingEpisodeIds,
      onSearchMissingPress
    } = this.props;

    onSearchMissingPress(missingEpisodeIds);
  };

  //
  // Render

  render() {
    const {
      selectedFilterKey,
      filters,
      hasSeries,
      missingEpisodeIds,
      isRssSyncExecuting,
      isSearchingForMissing,
      useCurrentPage,
      onRssSyncPress,
      onFilterSelect
    } = this.props;

    const {
      isCalendarLinkModalOpen,
      isOptionsModalOpen
    } = this.state;

    const isMeasured = this.state.width > 0;
    const PageComponent = hasSeries ? CalendarConnector : NoSeries;

    return (
      <PageContent title="Calendar">
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label="iCal Link"
              iconName={icons.CALENDAR}
              onPress={this.onGetCalendarLinkPress}
            />

            <PageToolbarSeparator />

            <PageToolbarButton
              label="RSS Sync"
              iconName={icons.RSS}
              isSpinning={isRssSyncExecuting}
              onPress={onRssSyncPress}
            />

            <PageToolbarButton
              label="Search for Missing"
              iconName={icons.SEARCH}
              isDisabled={!missingEpisodeIds.length}
              isSpinning={isSearchingForMissing}
              onPress={this.onSearchMissingPress}
            />
          </PageToolbarSection>

          <PageToolbarSection alignContent={align.RIGHT}>
            <PageToolbarButton
              label="Options"
              iconName={icons.POSTER}
              onPress={this.onOptionsPress}
            />

            <FilterMenu
              alignMenu={align.RIGHT}
              isDisabled={!hasSeries}
              selectedFilterKey={selectedFilterKey}
              filters={filters}
              customFilters={[]}
              onFilterSelect={onFilterSelect}
            />
          </PageToolbarSection>
        </PageToolbar>

        <PageContentBody
          className={styles.calendarPageBody}
          innerClassName={styles.calendarInnerPageBody}
        >
          <Measure
            whitelist={['width']}
            onMeasure={this.onMeasure}
          >
            {
              isMeasured ?
                <PageComponent
                  useCurrentPage={useCurrentPage}
                /> :
                <div />
            }
          </Measure>

          {
            hasSeries &&
              <LegendConnector />
          }
        </PageContentBody>

        <CalendarLinkModal
          isOpen={isCalendarLinkModalOpen}
          onModalClose={this.onGetCalendarLinkModalClose}
        />

        <CalendarOptionsModal
          isOpen={isOptionsModalOpen}
          onModalClose={this.onOptionsModalClose}
        />
      </PageContent>
    );
  }
}

CalendarPage.propTypes = {
  selectedFilterKey: PropTypes.string.isRequired,
  filters: PropTypes.arrayOf(PropTypes.object).isRequired,
  hasSeries: PropTypes.bool.isRequired,
  missingEpisodeIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  isRssSyncExecuting: PropTypes.bool.isRequired,
  isSearchingForMissing: PropTypes.bool.isRequired,
  useCurrentPage: PropTypes.bool.isRequired,
  onSearchMissingPress: PropTypes.func.isRequired,
  onDaysCountChange: PropTypes.func.isRequired,
  onRssSyncPress: PropTypes.func.isRequired,
  onFilterSelect: PropTypes.func.isRequired
};

export default CalendarPage;
