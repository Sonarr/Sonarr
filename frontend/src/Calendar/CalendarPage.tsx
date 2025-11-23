import React, {
  PropsWithChildren,
  useCallback,
  useEffect,
  useMemo,
  useState,
} from 'react';
import { useDispatch, useSelector } from 'react-redux';
import QueueDetailsProvider from 'Activity/Queue/Details/QueueDetailsProvider';
import * as commandNames from 'Commands/commandNames';
import FilterMenu from 'Components/Menu/FilterMenu';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import Episode from 'Episode/Episode';
import EpisodeFileProvider from 'EpisodeFile/EpisodeFileProvider';
import { useCustomFiltersList } from 'Filters/useCustomFilters';
import useMeasure from 'Helpers/Hooks/useMeasure';
import { align, icons } from 'Helpers/Props';
import NoSeries from 'Series/NoSeries';
import { executeCommand } from 'Store/Actions/commandActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import createSeriesCountSelector from 'Store/Selectors/createSeriesCountSelector';
import selectUniqueIds from 'Utilities/Object/selectUniqueIds';
import translate from 'Utilities/String/translate';
import Calendar from './Calendar';
import CalendarFilterModal from './CalendarFilterModal';
import CalendarMissingEpisodeSearchButton from './CalendarMissingEpisodeSearchButton';
import { setCalendarOption, useCalendarOption } from './calendarOptionsStore';
import CalendarLinkModal from './iCal/CalendarLinkModal';
import Legend from './Legend/Legend';
import CalendarOptionsModal from './Options/CalendarOptionsModal';
import useCalendar, {
  FILTERS,
  setCalendarDayCount,
  useCalendarPage,
} from './useCalendar';
import styles from './CalendarPage.css';

const MINIMUM_DAY_WIDTH = 120;

function CalendarPage() {
  const dispatch = useDispatch();

  const selectedFilterKey = useCalendarOption('selectedFilterKey');
  const { data } = useCalendar();

  useCalendarPage();

  const isRssSyncExecuting = useSelector(
    createCommandExecutingSelector(commandNames.RSS_SYNC)
  );
  const customFilters = useCustomFiltersList('calendar');
  const hasSeries = !!useSelector(createSeriesCountSelector());

  const [pageContentRef, { width }] = useMeasure();
  const [isCalendarLinkModalOpen, setIsCalendarLinkModalOpen] = useState(false);
  const [isOptionsModalOpen, setIsOptionsModalOpen] = useState(false);

  const isMeasured = width > 0;
  const PageComponent = hasSeries ? Calendar : NoSeries;

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

  const handleFilterSelect = useCallback((key: string | number) => {
    setCalendarOption('selectedFilterKey', key);
  }, []);

  const episodeIds = useMemo(() => {
    return selectUniqueIds<Episode, number>(data, 'id');
  }, [data]);

  const episodeFileIds = useMemo(() => {
    return selectUniqueIds<Episode, number>(data, 'episodeFileId');
  }, [data]);

  useEffect(() => {
    if (width === 0) {
      return;
    }

    const dayCount = Math.max(
      3,
      Math.min(7, Math.floor(width / MINIMUM_DAY_WIDTH))
    );

    setCalendarDayCount(dayCount);
  }, [width]);

  return (
    <CalendarPageProvider
      episodeIds={episodeIds}
      episodeFileIds={episodeFileIds}
    >
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

            <CalendarMissingEpisodeSearchButton />
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
              filters={FILTERS}
              customFilters={customFilters}
              filterModalConnectorComponent={CalendarFilterModal}
              onFilterSelect={handleFilterSelect}
            />
          </PageToolbarSection>
        </PageToolbar>

        <PageContentBody
          ref={pageContentRef}
          className={styles.calendarPageBody}
          innerClassName={styles.calendarInnerPageBody}
        >
          {isMeasured ? <PageComponent totalItems={0} /> : <div />}
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
    </CalendarPageProvider>
  );
}

export default CalendarPage;

function CalendarPageProvider({
  episodeIds,
  episodeFileIds,
  children,
}: PropsWithChildren<{ episodeIds: number[]; episodeFileIds: number[] }>) {
  return (
    <QueueDetailsProvider episodeIds={episodeIds}>
      <EpisodeFileProvider episodeFileIds={episodeFileIds}>
        {children}
      </EpisodeFileProvider>
    </QueueDetailsProvider>
  );
}
