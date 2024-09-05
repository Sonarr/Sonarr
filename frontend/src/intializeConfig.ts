export async function initializeConfig() {
  const initializeUrl = `${
    window.Sonarr.urlBase
  }/initialize.json?t=${Date.now()}`;
  const response = await fetch(initializeUrl);

  window.Sonarr = await response.json();
}
