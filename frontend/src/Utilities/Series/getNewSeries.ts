import Series, {
  MonitorNewItems,
  SeriesMonitor,
  SeriesType,
} from 'Series/Series';

interface NewSeriesPayload {
  rootFolderPath: string;
  monitor: SeriesMonitor;
  monitorNewItems: MonitorNewItems;
  qualityProfileId: number;
  seriesType: SeriesType;
  seasonFolder: boolean;
  tags: number[];
  searchForMissingEpisodes?: boolean;
  searchForCutoffUnmetEpisodes?: boolean;
}

function getNewSeries(series: Series, payload: NewSeriesPayload) {
  const {
    rootFolderPath,
    monitor,
    monitorNewItems,
    qualityProfileId,
    seriesType,
    seasonFolder,
    tags,
    searchForMissingEpisodes = false,
    searchForCutoffUnmetEpisodes = false,
  } = payload;

  const addOptions = {
    monitor,
    searchForMissingEpisodes,
    searchForCutoffUnmetEpisodes,
  };

  series.addOptions = addOptions;
  series.monitored = true;
  series.monitorNewItems = monitorNewItems;
  series.qualityProfileId = qualityProfileId;
  series.rootFolderPath = rootFolderPath;
  series.seriesType = seriesType;
  series.seasonFolder = seasonFolder;
  series.tags = tags;

  return series;
}

export default getNewSeries;
