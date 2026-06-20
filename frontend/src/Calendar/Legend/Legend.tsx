import React from 'react';
import { useCalendarOptions } from 'Calendar/calendarOptionsStore';
import { icons, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import LegendItem from './LegendItem';
import styles from './Legend.css';

function Legend() {
  const {
    showFinaleIcon,
    showSpecialIcon,
    showCutoffUnmetIcon,
    fullColorEvents,
  } = useCalendarOptions();

  const iconsToShow = [];

  if (showFinaleIcon) {
    iconsToShow.push(
      <LegendItem
        name={translate('SeasonFinale')}
        icon={icons.FINALE_SEASON}
        kind={kinds.WARNING}
        fullColorEvents={fullColorEvents}
        tooltip={translate('CalendarLegendSeriesFinaleTooltip')}
      />
    );

    iconsToShow.push(
      <LegendItem
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
      <LegendItem
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
      <LegendItem
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
          fullColorEvents={fullColorEvents}
        />

        <LegendItem
          status="unmonitored"
          tooltip={translate('CalendarLegendEpisodeUnmonitoredTooltip')}
          fullColorEvents={fullColorEvents}
        />
      </div>

      <div>
        <LegendItem
          status="onAir"
          name="On Air"
          tooltip={translate('CalendarLegendEpisodeOnAirTooltip')}
          fullColorEvents={fullColorEvents}
        />

        <LegendItem
          status="missing"
          tooltip={translate('CalendarLegendEpisodeMissingTooltip')}
          fullColorEvents={fullColorEvents}
        />
      </div>

      <div>
        <LegendItem
          status="downloading"
          tooltip={translate('CalendarLegendEpisodeDownloadingTooltip')}
          fullColorEvents={fullColorEvents}
        />

        <LegendItem
          status="downloaded"
          tooltip={translate('CalendarLegendEpisodeDownloadedTooltip')}
          fullColorEvents={fullColorEvents}
        />
      </div>

      <div>
        <LegendItem
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
