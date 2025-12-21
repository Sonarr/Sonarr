import ModelBase from 'App/ModelBase';
import useApiQuery from 'Helpers/Hooks/useApiQuery';

export interface OrganizePreviewModel extends ModelBase {
  seriesId: number;
  seasonNumber: number;
  episodeNumbers: number[];
  episodeFileId: number;
  existingPath: string;
  newPath: string;
}

const DEFAULT_ORGANIZE_PREVIEW: OrganizePreviewModel[] = [];

const useOrganizePreview = (seriesId: number, seasonNumber?: number) => {
  const queryParams: { seriesId: number; seasonNumber?: number } = { seriesId };

  if (seasonNumber != null) {
    queryParams.seasonNumber = seasonNumber;
  }

  const { data, ...result } = useApiQuery<OrganizePreviewModel[]>({
    path: '/rename',
    queryParams,
  });

  return {
    items: data ?? DEFAULT_ORGANIZE_PREVIEW,
    ...result,
  };
};

export default useOrganizePreview;
