import ModelBase from 'App/ModelBase';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import Quality from 'Quality/Quality';
import {
  useDeleteProvider,
  useManageProviderSettings,
  useProviderSettings,
} from 'Settings/useProviderSettings';
import { QualityProfileFormatItem } from 'typings/CustomFormat';
import translate from 'Utilities/String/translate';

export interface QualityProfileQualityItem {
  quality: Quality;
  allowed: boolean;
  minSize: number | null;
  maxSize: number | null;
  preferredSize: number | null;
}

export interface QualityProfileGroup {
  id: number;
  items: QualityProfileQualityItem[];
  allowed: boolean;
  name: string;
}

export type QualityProfileItems = (
  | QualityProfileQualityItem
  | QualityProfileGroup
)[];

export interface QualityProfileModel extends ModelBase {
  name: string;
  upgradeAllowed: boolean;
  cutoff: number;
  items: QualityProfileItems;
  minFormatScore: number;
  cutoffFormatScore: number;
  minUpgradeFormatScore: number;
  formatItems: QualityProfileFormatItem[];
}

const PATH = '/qualityprofile';

export const useQualityProfile = (id: number | undefined) => {
  const { data } = useQualityProfiles();

  if (id === undefined) {
    return undefined;
  }

  return data.find((profile) => profile.id === id);
};

export const useQualityProfilesData = () => {
  const { data } = useQualityProfiles();

  return data;
};

export const useQualityProfiles = () => {
  return useProviderSettings<QualityProfileModel>({
    path: PATH,
    queryOptions: {
      gcTime: Infinity,
      staleTime: 5 * 60 * 1000,
    },
  });
};

export const useManageQualityProfile = (
  id: number | undefined,
  cloneId: number | undefined
) => {
  const { schema, isSchemaFetching, isSchemaFetched, schemaError } =
    useQualityProfileSchema(cloneId == null);

  const profile = useQualityProfile(cloneId);

  if (cloneId && !profile) {
    throw new Error(`Quality Profile with ID ${cloneId} not found`);
  }

  const manage = useManageProviderSettings<QualityProfileModel>(
    id,
    cloneId && profile
      ? {
          ...profile,
          id: 0,
          name: translate('DefaultNameCopiedProfile', {
            name: profile.name,
          }),
        }
      : schema,
    PATH
  );

  return {
    ...manage,
    isSchemaFetching: cloneId ? false : isSchemaFetching,
    isSchemaFetched: cloneId ? true : isSchemaFetched,
    schemaError: cloneId ? undefined : schemaError,
  };
};

export const useDeleteQualityProfile = (id: number) => {
  const result = useDeleteProvider<QualityProfileModel>(id, PATH);

  return {
    ...result,
    deleteQualityProfile: result.deleteProvider,
  };
};

export const useQualityProfileSchema = (enabled: boolean) => {
  const { isFetching, isFetched, error, data } =
    useApiQuery<QualityProfileModel>({
      path: `${PATH}/schema`,
      queryOptions: {
        enabled,
      },
    });

  return {
    isSchemaFetching: isFetching,
    isSchemaFetched: isFetched,
    schemaError: error,
    schema: data ?? ({} as QualityProfileModel),
  };
};
