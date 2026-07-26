import getFinaleTypeName from 'Episode/getFinaleTypeName';
import translate from 'Utilities/String/translate';

interface CalendarEventStatusDetailsOptions {
  missingAbsoluteNumber: boolean;
  unverifiedSceneNumbering?: boolean;
  qualityCutoffNotMet: boolean;
  seasonNumber: number;
  episodeNumber: number;
  finaleType?: string;
}

export default function getCalendarEventStatusDetails({
  missingAbsoluteNumber,
  unverifiedSceneNumbering,
  qualityCutoffNotMet,
  seasonNumber,
  episodeNumber,
  finaleType,
}: CalendarEventStatusDetailsOptions) {
  const labels: string[] = [];

  if (missingAbsoluteNumber) {
    labels.push(translate('EpisodeMissingAbsoluteNumber'));
  } else if (unverifiedSceneNumbering) {
    labels.push(translate('SceneNumberNotVerified'));
  }

  if (qualityCutoffNotMet) {
    labels.push(translate('QualityCutoffNotMet'));
  }

  if (episodeNumber === 1 && seasonNumber > 0) {
    labels.push(
      seasonNumber === 1
        ? translate('SeriesPremiere')
        : translate('SeasonPremiere')
    );
  }

  if (finaleType) {
    const finaleTypeName = getFinaleTypeName(finaleType);

    if (finaleTypeName) {
      labels.push(finaleTypeName);
    }
  }

  if (episodeNumber === 0 || seasonNumber === 0) {
    labels.push(translate('Special'));
  }

  return labels;
}
