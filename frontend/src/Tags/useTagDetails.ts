import ModelBase from 'App/ModelBase';
import useApiQuery from 'Helpers/Hooks/useApiQuery';

const DEFAULT_TAG_DETAILS: TagDetail[] = [];

export interface TagDetail extends ModelBase {
  label: string;
  autoTagIds: number[];
  delayProfileIds: number[];
  downloadClientIds: [];
  importListIds: number[];
  indexerIds: number[];
  notificationIds: number[];
  restrictionIds: number[];
  excludedReleaseProfileIds: number[];
  seriesIds: number[];
}

const useTagDetails = () => {
  const { queryKey, ...result } = useApiQuery<TagDetail[]>({
    path: '/tag/detail',
  });

  return {
    ...result,
    data: result.data ?? DEFAULT_TAG_DETAILS,
  };
};

export default useTagDetails;

export const useTagDetail = (id: number) => {
  const { data: tagDetails } = useTagDetails();

  return (
    tagDetails.find((tagDetail) => tagDetail.id === id) ?? {
      delayProfileIds: [],
      importListIds: [],
      notificationIds: [],
      restrictionIds: [],
      excludedReleaseProfileIds: [],
      indexerIds: [],
      downloadClientIds: [],
      autoTagIds: [],
      seriesIds: [],
    }
  );
};
