function formatRuntime(runtime: number) {
  if (!runtime) {
    return '';
  }

  const hours = Math.floor(runtime / 60);
  const minutes = runtime % 60;
  const result = [];

  if (hours) {
    result.push(`${hours}h`);
  }

  if (minutes) {
    result.push(`${minutes}m`);
  }

  return result.join(' ');
}

export default formatRuntime;
