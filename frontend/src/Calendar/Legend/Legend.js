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
        name="Finale"
        icon={icons.INFO}
        kind={fullColorEvents ? kinds.DEFAULT : kinds.WARNING}
        tooltip={translate('CalendarLegendSeriesFinaleTooltip')}
      />
    );
  }

  if (showSpecialIcon) {
    iconsToShow.push(
      <LegendIconItem
        name="Special"
        icon={icons.INFO}
        kind={kinds.PINK}
        darken={fullColorEvents}
        tooltip={translate('SpecialEpisode')}
      />
    );
  }

  if (showCutoffUnmetIcon) {
    iconsToShow.push(
      <LegendIconItem
        name="Cutoff Not Met"
        icon={icons.EPISODE_FILE}
        kind={fullColorEvents ? kinds.DEFAULT : kinds.WARNING}
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
          name="Premiere"
          icon={icons.INFO}
          kind={kinds.INFO}
          darken={true}
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
