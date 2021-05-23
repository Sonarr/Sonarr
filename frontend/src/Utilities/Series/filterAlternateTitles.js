
function filterAlternateTitles(alternateTitles, seriesTitle, useSceneNumbering, seasonNumber, sceneSeasonNumber) {
  const globalTitles = [];
  const seasonTitles = [];

  if (alternateTitles) {
    alternateTitles.forEach((alternateTitle) => {
      if (alternateTitle.sceneOrigin === 'unknown' || alternateTitle.sceneOrigin === 'unknown:tvdb') {
        return;
      }

      if (alternateTitle.sceneOrigin === 'mixed') {
        // For now filter out 'mixed' from the UI, the user will get an rejection during manual search.
        return;
      }

      const hasAltSeasonNumber = (alternateTitle.seasonNumber !== -1 && alternateTitle.seasonNumber !== undefined);
      const hasAltSceneSeasonNumber = (alternateTitle.sceneSeasonNumber !== -1 && alternateTitle.sceneSeasonNumber !== undefined);

      // Global alias that should be displayed global
      if (!hasAltSeasonNumber && !hasAltSceneSeasonNumber &&
          (alternateTitle.title !== seriesTitle) &&
          (!alternateTitle.sceneOrigin || !useSceneNumbering)) {
        globalTitles.push(alternateTitle);
        return;
      }

      // Global alias that should be displayed per episode
      if (!hasAltSeasonNumber && !hasAltSceneSeasonNumber && alternateTitle.sceneOrigin && useSceneNumbering) {
        seasonTitles.push(alternateTitle);
        return;
      }

      // Apply the alternative mapping (release to scene)
      const mappedAltSeasonNumber = hasAltSeasonNumber ? alternateTitle.seasonNumber : alternateTitle.sceneSeasonNumber;
      // Select scene or tvdb on the episode
      const mappedSeasonNumber = alternateTitle.sceneOrigin === 'tvdb' ? seasonNumber : sceneSeasonNumber;

      if (mappedSeasonNumber !== undefined && mappedSeasonNumber === mappedAltSeasonNumber) {
        seasonTitles.push(alternateTitle);
        return;
      }
    });
  }

  if (seasonNumber === undefined) {
    return globalTitles;
  }

  return seasonTitles;
}

export default filterAlternateTitles;
