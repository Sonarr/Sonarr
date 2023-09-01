import translate from 'Utilities/String/translate';

function formatRuntime(runtime: number) {
  if (!runtime) {
    return '';
  }

  const hours = Math.floor(runtime / 60);
  const minutes = runtime % 60;
  const result = [];

  if (hours) {
    result.push(translate('FormatRuntimeHours', { hours }));
  }

  if (minutes) {
    result.push(translate('FormatRuntimeMinutes', { minutes }));
  }

  return result.join(' ');
}

export default formatRuntime;
