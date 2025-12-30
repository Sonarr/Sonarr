import { useMemo } from 'react';
import ModelBase from 'App/ModelBase';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import Provider from 'typings/Provider';

type ProviderWithPresets<T> = T & {
  presets: T[];
};

export interface SelectedSchema {
  implementation: string;
  implementationName: string;
  presetName?: string;
}

export const useProviderSchema = <T extends ModelBase>(
  path: string,
  enabled: boolean = true
) => {
  const { isFetching, isFetched, error, data } = useApiQuery<T[]>({
    path: `${path}/schema`,
    queryOptions: {
      enabled,
    },
  });

  return {
    isSchemaFetching: isFetching,
    isSchemaFetched: isFetched,
    schemaError: error,
    schema: data ?? ([] as T[]),
  };
};

export const useSelectedSchema = <T extends Provider>(
  path: string,
  selectedSchema: SelectedSchema | undefined
) => {
  const { schema } = useProviderSchema<T>(path, selectedSchema != null);

  return useMemo(() => {
    if (!selectedSchema) {
      return undefined;
    }

    const selected = schema.find(
      (s: T) => s.implementation === selectedSchema.implementation
    );

    if (!selected) {
      throw new Error(
        `Schema with implementation ${selectedSchema.implementation} not found`
      );
    }

    if (selectedSchema.presetName == null) {
      return selected;
    }

    const preset =
      'presets' in selected
        ? (selected as ProviderWithPresets<T>).presets?.find(
            (p: T & { name: string }) => p.name === selectedSchema.presetName
          )
        : undefined;

    if (!preset) {
      throw new Error(
        `Preset with name ${selectedSchema.presetName} not found for implementation ${selectedSchema.implementation}`
      );
    }

    return preset;
  }, [schema, selectedSchema]);
};
