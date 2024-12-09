import React from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import { icons, kinds } from 'Helpers/Props';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import translate from 'Utilities/String/translate';
import LegendIconItem from './LegendIconItem';
import LegendItem from './LegendItem';
import styles from './Legend.css';

function Legend() {
  const view = useSelector((state: AppState) => state.calendar.view);
  const {
    showFinaleIcon,
    showSpecialIcon,
    showCutoffUnmetIcon,
    fullColorEvents,
  } = useSelector((state: AppState) => state.calendar.options);
  const { enableColorImpairedMode } = useSelector(createUISettingsSelector());

  const iconsToShow = [];
  const isAgendaView = view === 'agenda';

  if (showFinaleIcon) {
    iconsToShow.push(
      <LegendIconItem
        name={translate('SeasonFinale')}
        icon={icons.FINALE_SEASON}
        kind={kinds.WARNING}
        fullColorEvents={fullColorEvents}
        tooltip={translate('CalendarLegendSeriesFinaleTooltip')}
      />
    );

    iconsToShow.push(
      <LegendIconItem
        name={translate('SeriesFinale')}
        icon={icons.FINALE_SERIES}
        kind={kinds.DANGER}
        fullColorEvents={fullColorEvents}
        tooltip={translate('CalendarLegendSeriesFinaleTooltip')}
      />
    );
  }

  if (showSpecialIcon) {
    iconsToShow.push(
      <LegendIconItem
        name={translate('Special')}
        icon={icons.INFO}
        kind={kinds.PINK}
        fullColorEvents={fullColorEvents}
        tooltip={translate('SpecialEpisode')}
      />
    );
  }

  if (showCutoffUnmetIcon) {
    iconsToShow.push(
      <LegendIconItem
        name={translate('CutoffNotMet')}
        icon={icons.EPISODE_FILE}
        kind={kinds.WARNING}
        fullColorEvents={fullColorEvents}
        tooltip={translate('QualityCutoffNotMet')}
      />
    );
  }

  return (
    <div className={styles.legend}>
      <div>
        <LegendItem
          status="unaired"
          tooltip={translate('CalendarLegendEpisodeUnairedTooltip')}
          isAgendaView={isAgendaView}
          fullColorEvents={fullColorEvents}
          colorImpairedMode={enableColorImpairedMode}
        />

        <LegendItem
          status="unmonitored"
          tooltip={translate('CalendarLegendEpisodeUnmonitoredTooltip')}
          isAgendaView={isAgendaView}
          fullColorEvents={fullColorEvents}
          colorImpairedMode={enableColorImpairedMode}
        />
      </div>

      <div>
        <LegendItem
          status="onAir"
          name="On Air"
          tooltip={translate('CalendarLegendEpisodeOnAirTooltip')}
          isAgendaView={isAgendaView}
          fullColorEvents={fullColorEvents}
          colorImpairedMode={enableColorImpairedMode}
        />

        <LegendItem
          status="missing"
          tooltip={translate('CalendarLegendEpisodeMissingTooltip')}
          isAgendaView={isAgendaView}
          fullColorEvents={fullColorEvents}
          colorImpairedMode={enableColorImpairedMode}
        />
      </div>

      <div>
        <LegendItem
          status="downloading"
          tooltip={translate('CalendarLegendEpisodeDownloadingTooltip')}
          isAgendaView={isAgendaView}
          fullColorEvents={fullColorEvents}
          colorImpairedMode={enableColorImpairedMode}
        />

        <LegendItem
          status="downloaded"
          tooltip={translate('CalendarLegendEpisodeDownloadedTooltip')}
          isAgendaView={isAgendaView}
          fullColorEvents={fullColorEvents}
          colorImpairedMode={enableColorImpairedMode}
        />
      </div>

      <div>
        <LegendIconItem
          name={translate('Premiere')}
          icon={icons.PREMIERE}
          kind={kinds.INFO}
          fullColorEvents={fullColorEvents}
          tooltip={translate('CalendarLegendSeriesPremiereTooltip')}
        />

        {iconsToShow[0]}
      </div>

      {iconsToShow.length > 1 ? (
        <div>
          {iconsToShow[1]}
          {iconsToShow[2]}
        </div>
      ) : null}
      {iconsToShow.length > 3 ? <div>{iconsToShow[3]}</div> : null}
    </div>
  );
}

export default Legend;
