import translate from 'Utilities/String/translate';

export default function formatSeasonNumbers(seasonNumbers: number[]) {
  const first = seasonNumbers[0];
  const last = seasonNumbers[seasonNumbers.length - 1];

  if (seasonNumbers.length === 1) {
    return translate('SeasonNumberToken', { seasonNumber: first });
  }

  if (last - first + 1 === seasonNumbers.length) {
    return translate('SeasonRangeToken', {
      firstSeasonNumber: first,
      lastSeasonNumber: last,
    });
  }

  return translate('SeasonListToken', {
    seasonNumbers: seasonNumbers.join(', '),
  });
}
