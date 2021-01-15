
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

      if (!hasAltSeasonNumber && !hasAltSceneSeasonNumber &&
          (alternateTitle.title !== seriesTitle) &&
          (!alternateTitle.sceneOrigin || !useSceneNumbering)) {
        globalTitles.push(alternateTitle);
        return;
      }

      if ((sceneSeasonNumber !== undefined && sceneSeasonNumber === alternateTitle.sceneSeasonNumber) ||
          (seasonNumber !== undefined && seasonNumber === alternateTitle.seasonNumber) ||
          (!hasAltSeasonNumber && !hasAltSceneSeasonNumber && alternateTitle.sceneOrigin && useSceneNumbering)) {
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
