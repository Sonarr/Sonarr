import PropTypes from 'prop-types';
import React from 'react';
import { icons, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import LegendIconItem from './LegendIconItem';
import LegendItem from './LegendItem';
import styles from './Legend.css';

function Legend(props) {
  const {
    view,
    showFinaleIcon,
    showSpecialIcon,
    showCutoffUnmetIcon,
    fullColorEvents,
    colorImpairedMode
  } = props;

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
          colorImpairedMode={colorImpairedMode}
        />

        <LegendItem
          status="unmonitored"
          tooltip={translate('CalendarLegendEpisodeUnmonitoredTooltip')}
          isAgendaView={isAgendaView}
          fullColorEvents={fullColorEvents}
          colorImpairedMode={colorImpairedMode}
        />
      </div>

      <div>
        <LegendItem
          status="onAir"
          name="On Air"
          tooltip={translate('CalendarLegendEpisodeOnAirTooltip')}
          isAgendaView={isAgendaView}
          fullColorEvents={fullColorEvents}
          colorImpairedMode={colorImpairedMode}
        />

        <LegendItem
          status="missing"
          tooltip={translate('CalendarLegendEpisodeMissingTooltip')}
          isAgendaView={isAgendaView}
          fullColorEvents={fullColorEvents}
          colorImpairedMode={colorImpairedMode}
        />
      </div>

      <div>
        <LegendItem
          status="downloading"
          tooltip={translate('CalendarLegendEpisodeDownloadingTooltip')}
          isAgendaView={isAgendaView}
          fullColorEvents={fullColorEvents}
          colorImpairedMode={colorImpairedMode}
        />

        <LegendItem
          status="downloaded"
          tooltip={translate('CalendarLegendEpisodeDownloadedTooltip')}
          isAgendaView={isAgendaView}
          fullColorEvents={fullColorEvents}
          colorImpairedMode={colorImpairedMode}
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

      {
        iconsToShow.length > 1 &&
          <div>
            {iconsToShow[1]}
            {iconsToShow[2]}
          </div>
      }
      {
        iconsToShow.length > 3 &&
          <div>
            {iconsToShow[3]}
          </div>
      }
    </div>
  );
}

Legend.propTypes = {
  view: PropTypes.string.isRequired,
  showFinaleIcon: PropTypes.bool.isRequired,
  showSpecialIcon: PropTypes.bool.isRequired,
  showCutoffUnmetIcon: PropTypes.bool.isRequired,
  fullColorEvents: PropTypes.bool.isRequired,
  colorImpairedMode: PropTypes.bool.isRequired
};

export default Legend;
