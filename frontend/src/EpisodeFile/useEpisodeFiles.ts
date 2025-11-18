import { useQueryClient } from '@tanstack/react-query';
import { EpisodeEntity, getQueryKey } from 'Episode/useEpisode';
import useApiMutation from 'Helpers/Hooks/useApiMutation';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import { EpisodeFile } from './EpisodeFile';

const DEFAULT_EPISODE_FILES: EpisodeFile[] = [];

interface SeriesEpisodeFiles {
  seriesId: number;
}

interface EpisodeFileIds {
  episodeFileIds: number[];
}

export type EpisodeFileFilter = SeriesEpisodeFiles | EpisodeFileIds;

const useEpisodeFiles = (params: EpisodeFileFilter) => {
  const result = useApiQuery<EpisodeFile[]>({
    path: '/episodeFile',
    queryParams: { ...params },
    queryOptions: {
      enabled:
        ('seriesId' in params && params.seriesId !== undefined) ||
        ('episodeFileIds' in params && params.episodeFileIds?.length > 0),
    },
  });

  return {
    ...result,
    data: result.data ?? DEFAULT_EPISODE_FILES,
    hasEpisodeFiles: !!result.data?.length,
  };
};

export default useEpisodeFiles;

export const useDeleteEpisodeFile = (
  id: number,
  episodeEntity: EpisodeEntity
) => {
  const queryClient = useQueryClient();

  const { mutate, error, isPending } = useApiMutation<unknown, void>({
    path: `/episodeFile/${id}`,
    method: 'DELETE',
    mutationOptions: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ['/episodeFile'] });
        queryClient.invalidateQueries({
          queryKey: [getQueryKey(episodeEntity)],
        });
      },
    },
  });

  return {
    deleteEpisodeFile: mutate,
    isDeleting: isPending,
    deleteError: error,
  };
};

export const useDeleteEpisodeFiles = () => {
  const queryClient = useQueryClient();

  const { mutate, error, isPending } = useApiMutation<unknown, EpisodeFileIds>({
    path: '/episodeFile/bulk',
    method: 'DELETE',
    mutationOptions: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ['/episodeFile'] });
        queryClient.invalidateQueries({ queryKey: ['/episode'] });
      },
    },
  });

  return {
    deleteEpisodeFiles: mutate,
    isDeleting: isPending,
    deleteError: error,
  };
};

export const useUpdateEpisodeFiles = () => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error } = useApiMutation<
    unknown,
    Partial<EpisodeFile>[]
  >({
    path: '/episodeFile/bulk',
    method: 'PUT',
    mutationOptions: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ['/episodeFile'] });
      },
    },
  });

  return {
    updateEpisodeFiles: mutate,
    isUpdating: isPending,
    updateError: error,
  };
};
