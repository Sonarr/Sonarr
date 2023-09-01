import padNumber from 'Utilities/Number/padNumber';
import translate from 'Utilities/String/translate';

export default function formatSeason(
  seasonNumber: number,
  shortFormat?: boolean
) {
  if (seasonNumber === 0) {
    return translate('Specials');
  }

  if (seasonNumber > 0) {
    return shortFormat
      ? `S${padNumber(seasonNumber, 2)}`
      : translate('SeasonNumberToken', { seasonNumber });
  }

  return null;
}
