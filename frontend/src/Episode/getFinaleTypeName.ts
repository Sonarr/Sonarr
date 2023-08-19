import translate from 'Utilities/String/translate';

export default function getFinaleTypeName(finaleType?: string): string | null {
  switch (finaleType) {
    case 'series':
      return translate('SeriesFinale');
    case 'season':
      return translate('SeasonFinale');
    case 'midseason':
      return translate('MidseasonFinale');
    default:
      return null;
  }
}
