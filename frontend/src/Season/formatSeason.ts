import translate from 'Utilities/String/translate';

export default function formatSeason(seasonNumber: number) {
  if (seasonNumber === 0) {
    return translate('Specials');
  }

  if (seasonNumber > 0) {
    return translate('SeasonNumberToken', { seasonNumber });
  }

  return null;
}
