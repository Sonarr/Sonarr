import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useCallback, useMemo } from 'react';
import ModelBase from 'App/ModelBase';
import {
  useDeleteProvider,
  useManageProviderSettings,
  useProviderSettings,
} from 'Settings/useProviderSettings';
import fetchJson, { ApiError } from 'Utilities/Fetch/fetchJson';
import getQueryPath from 'Utilities/Fetch/getQueryPath';
import getQueryString from 'Utilities/Fetch/getQueryString';

export type DelayProfileProtocol = 'unknown' | 'usenet' | 'torrent';

export interface DelayProfile extends ModelBase {
  enableUsenet: boolean;
  enableTorrent: boolean;
  preferredProtocol: DelayProfileProtocol;
  usenetDelay: number;
  torrentDelay: number;
  bypassIfHighestQuality: boolean;
  bypassIfAboveCustomFormatScore: boolean;
  minimumCustomFormatScore: number;
  order: number;
  tags: number[];
}

interface ReorderDelayProfilePayload {
  id: number;
  after?: number;
}

const PATH = '/delayprofile';

const DEFAULT_DELAY_PROFILE: DelayProfile = {
  id: 0,
  enableUsenet: true,
  enableTorrent: true,
  preferredProtocol: 'usenet',
  usenetDelay: 0,
  torrentDelay: 0,
  bypassIfHighestQuality: false,
  bypassIfAboveCustomFormatScore: false,
  minimumCustomFormatScore: 0,
  order: 0,
  tags: [],
};

export const useDelayProfiles = () => {
  return useProviderSettings<DelayProfile>({
    path: PATH,
  });
};

export const useSortedDelayProfiles = () => {
  const result = useDelayProfiles();

  const { defaultProfile, items } = useMemo(() => {
    const sorted = [...result.data].sort((a, b) => a.order - b.order);

    return sorted.reduce<{
      defaultProfile: DelayProfile | null;
      items: ReadonlyArray<DelayProfile>;
    }>(
      (acc, item) => {
        if (item.id === 1) {
          acc.defaultProfile = item;
        } else {
          acc.items = [...acc.items, item];
        }

        return acc;
      },
      { defaultProfile: null, items: [] }
    );
  }, [result.data]);

  return { ...result, defaultProfile, items };
};

export const useDelayProfilesWithIds = (ids: number[]) => {
  const { data } = useDelayProfiles();

  return data.filter((profile) => ids.includes(profile.id));
};

export const useDelayProfile = (id: number | undefined) => {
  const { data } = useDelayProfiles();

  if (id === undefined) {
    return undefined;
  }

  return data.find((profile) => profile.id === id);
};

export const useManageDelayProfile = (id: number | undefined) => {
  return useManageProviderSettings<DelayProfile>(
    id,
    DEFAULT_DELAY_PROFILE,
    PATH
  );
};

export const useDeleteDelayProfile = (id: number) => {
  const result = useDeleteProvider<DelayProfile>(id, PATH);

  return {
    ...result,
    deleteDelayProfile: result.deleteProvider,
  };
};

export const useReorderDelayProfile = () => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error } = useMutation<
    DelayProfile[],
    ApiError,
    ReorderDelayProfilePayload
  >({
    mutationFn: async ({ id, after }) => {
      return fetchJson<DelayProfile[], void>({
        path:
          getQueryPath(`${PATH}/reorder/${id}`) +
          getQueryString(after == null ? {} : { after }),
        method: 'PUT',
        headers: {
          'X-Api-Key': window.Sonarr.apiKey,
          'X-Sonarr-Client': 'Sonarr',
        },
      });
    },
    onSuccess: (updatedProfiles) => {
      queryClient.setQueryData<DelayProfile[]>([PATH], updatedProfiles);
    },
  });

  const reorderDelayProfile = useCallback(
    (payload: ReorderDelayProfilePayload) => mutate(payload),
    [mutate]
  );

  return {
    reorderDelayProfile,
    isReordering: isPending,
    reorderError: error,
  };
};
