import moment from 'moment';
import React, { useCallback, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import * as commandNames from 'Commands/commandNames';
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
import {
  searchMissing,
  setCalendarDaysCount,
  setCalendarFilter,
} from 'Store/Actions/calendarActions';
import { executeCommand } from 'Store/Actions/commandActions';
import { createCustomFiltersSelector } from 'Store/Selectors/createClientSideCollectionSelector';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import createSeriesCountSelector from 'Store/Selectors/createSeriesCountSelector';
import { isCommandExecuting } from 'Utilities/Command';
import isBefore from 'Utilities/Date/isBefore';
import translate from 'Utilities/String/translate';
import Calendar from './Calendar';
import CalendarFilterModal from './CalendarFilterModal';
import CalendarLinkModal from './iCal/CalendarLinkModal';
import Legend from './Legend/Legend';
import CalendarOptionsModal from './Options/CalendarOptionsModal';
import styles from './CalendarPage.css';

const MINIMUM_DAY_WIDTH = 120;

function createMissingEpisodeIdsSelector() {
  return createSelector(
    (state: AppState) => state.calendar.start,
    (state: AppState) => state.calendar.end,
    (state: AppState) => state.calendar.items,
    (state: AppState) => state.queue.details.items,
    (start, end, episodes, queueDetails) => {
      return episodes.reduce<number[]>((acc, episode) => {
        const airDateUtc = episode.airDateUtc;

        if (
          !episode.episodeFileId &&
          moment(airDateUtc).isAfter(start) &&
          moment(airDateUtc).isBefore(end) &&
          isBefore(episode.airDateUtc) &&
          !queueDetails.some(
            (details) => !!details.episode && details.episode.id === episode.id
          )
        ) {
          acc.push(episode.id);
        }

        return acc;
      }, []);
    }
  );
}

function createIsSearchingSelector() {
  return createSelector(
    (state: AppState) => state.calendar.searchMissingCommandId,
    createCommandsSelector(),
    (searchMissingCommandId, commands) => {
      if (searchMissingCommandId == null) {
        return false;
      }

      return isCommandExecuting(
        commands.find((command) => {
          return command.id === searchMissingCommandId;
        })
      );
    }
  );
}

function CalendarPage() {
  const dispatch = useDispatch();

  const { selectedFilterKey, filters } = useSelector(
    (state: AppState) => state.calendar
  );
  const missingEpisodeIds = useSelector(createMissingEpisodeIdsSelector());
  const isSearchingForMissing = useSelector(createIsSearchingSelector());
  const isRssSyncExecuting = useSelector(
    createCommandExecutingSelector(commandNames.RSS_SYNC)
  );
  const customFilters = useSelector(createCustomFiltersSelector('calendar'));
  const hasSeries = !!useSelector(createSeriesCountSelector());

  const [isCalendarLinkModalOpen, setIsCalendarLinkModalOpen] = useState(false);
  const [isOptionsModalOpen, setIsOptionsModalOpen] = useState(false);
  const [width, setWidth] = useState(0);

  const isMeasured = width > 0;
  const PageComponent = hasSeries ? Calendar : NoSeries;

  const handleMeasure = useCallback(
    ({ width: newWidth }: { width: number }) => {
      setWidth(newWidth);

      const dayCount = Math.max(
        3,
        Math.min(7, Math.floor(newWidth / MINIMUM_DAY_WIDTH))
      );

      dispatch(setCalendarDaysCount({ dayCount }));
    },
    [dispatch]
  );

  const handleGetCalendarLinkPress = useCallback(() => {
    setIsCalendarLinkModalOpen(true);
  }, []);

  const handleGetCalendarLinkModalClose = useCallback(() => {
    setIsCalendarLinkModalOpen(false);
  }, []);

  const handleOptionsPress = useCallback(() => {
    setIsOptionsModalOpen(true);
  }, []);

  const handleOptionsModalClose = useCallback(() => {
    setIsOptionsModalOpen(false);
  }, []);

  const handleRssSyncPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: commandNames.RSS_SYNC,
      })
    );
  }, [dispatch]);

  const handleSearchMissingPress = useCallback(() => {
    dispatch(searchMissing({ episodeIds: missingEpisodeIds }));
  }, [missingEpisodeIds, dispatch]);

  const handleFilterSelect = useCallback(
    (key: string) => {
      dispatch(setCalendarFilter({ selectedFilterKey: key }));
    },
    [dispatch]
  );

  return (
    <PageContent title={translate('Calendar')}>
      <PageToolbar>
        <PageToolbarSection>
          <PageToolbarButton
            label={translate('ICalLink')}
            iconName={icons.CALENDAR}
            onPress={handleGetCalendarLinkPress}
          />

          <PageToolbarSeparator />

          <PageToolbarButton
            label={translate('RssSync')}
            iconName={icons.RSS}
            isSpinning={isRssSyncExecuting}
            onPress={handleRssSyncPress}
          />

          <PageToolbarButton
            label={translate('SearchForMissing')}
            iconName={icons.SEARCH}
            isDisabled={!missingEpisodeIds.length}
            isSpinning={isSearchingForMissing}
            onPress={handleSearchMissingPress}
          />
        </PageToolbarSection>

        <PageToolbarSection alignContent={align.RIGHT}>
          <PageToolbarButton
            label={translate('Options')}
            iconName={icons.POSTER}
            onPress={handleOptionsPress}
          />

          <FilterMenu
            alignMenu={align.RIGHT}
            isDisabled={!hasSeries}
            selectedFilterKey={selectedFilterKey}
            filters={filters}
            customFilters={customFilters}
            filterModalConnectorComponent={CalendarFilterModal}
            onFilterSelect={handleFilterSelect}
          />
        </PageToolbarSection>
      </PageToolbar>

      <PageContentBody
        className={styles.calendarPageBody}
        innerClassName={styles.calendarInnerPageBody}
      >
        <Measure whitelist={['width']} onMeasure={handleMeasure}>
          {isMeasured ? <PageComponent totalItems={0} /> : <div />}
        </Measure>

        {hasSeries && <Legend />}
      </PageContentBody>

      <CalendarLinkModal
        isOpen={isCalendarLinkModalOpen}
        onModalClose={handleGetCalendarLinkModalClose}
      />

      <CalendarOptionsModal
        isOpen={isOptionsModalOpen}
        onModalClose={handleOptionsModalClose}
      />
    </PageContent>
  );
}

export default CalendarPage;
