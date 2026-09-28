import { NewCommandBody } from 'Commands/Command';
import { PropertyFilter } from 'Filters/Filter';

export interface WantedSearchCommandBody extends NewCommandBody {
  name: string;
  seriesIds?: number[];
  qualityProfileIds?: number[];
  seriesType?: string[];
  seriesTags?: number[];
  quality?: number[];
}

const toWantedSearchCommandBody = (
  name: string,
  filters: PropertyFilter[]
): WantedSearchCommandBody => {
  const body: WantedSearchCommandBody = { name };

  for (const f of filters) {
    const value = Array.isArray(f.value) ? f.value : [f.value];

    if (f.key === 'seriesIds') {
      body.seriesIds = value as number[];
    } else if (f.key === 'qualityProfileIds') {
      body.qualityProfileIds = value as number[];
    } else if (f.key === 'seriesType') {
      body.seriesType = value as string[];
    } else if (f.key === 'seriesTags') {
      body.seriesTags = value as number[];
    } else if (f.key === 'quality') {
      body.quality = value as number[];
    }
  }

  return body;
};

export default toWantedSearchCommandBody;
