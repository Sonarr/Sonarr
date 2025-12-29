import ModelBase from 'App/ModelBase';
import {
  useDeleteProvider,
  useManageProviderSettings,
  useProviderSettings,
} from 'Settings/useProviderSettings';

export interface ReleaseProfileModel extends ModelBase {
  name: string;
  enabled: boolean;
  required: string[];
  ignored: string[];
  indexerId: number;
  tags: number[];
  excludedTags: number[];
}

const PATH = '/releaseprofile';

const NEW_RELEASE_PROFILE: ReleaseProfileModel = {
  id: 0,
  name: '',
  enabled: true,
  required: [],
  ignored: [],
  indexerId: 0,
  tags: [],
  excludedTags: [],
};

export const useReleaseProfilesWithIds = (ids: number[]) => {
  const allReleaseProfiles = useReleaseProfiles();

  return allReleaseProfiles.data.filter((releaseProfiles) =>
    ids.includes(releaseProfiles.id)
  );
};

export const useReleaseProfiles = () => {
  return useProviderSettings<ReleaseProfileModel>(PATH);
};

export const useManageReleaseProfile = (id: number) => {
  return useManageProviderSettings<ReleaseProfileModel>(
    id,
    NEW_RELEASE_PROFILE,
    PATH
  );
};

export const useDeleteReleaseProfile = (id: number) => {
  const result = useDeleteProvider<ReleaseProfileModel>(id, PATH);

  return {
    ...result,
    deleteReleaseProfile: result.deleteProvider,
  };
};
