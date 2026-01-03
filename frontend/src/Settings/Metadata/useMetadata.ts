import { useMemo } from 'react';
import {
  SelectedSchema,
  useProviderSchema,
  useSelectedSchema,
} from 'Settings/useProviderSchema';
import {
  useDeleteProvider,
  useManageProviderSettings,
  useProviderSettings,
} from 'Settings/useProviderSettings';
import Provider from 'typings/Provider';
import { sortByProp } from 'Utilities/Array/sortByProp';

export interface MetadataModel extends Provider {
  enable: boolean;
  tags: number[];
}

const PATH = '/metadata';

export const useMetadataWithIds = (ids: number[]) => {
  const allMetadata = useMetadataData();

  return allMetadata.filter((metadata) => ids.includes(metadata.id));
};

export const useMetadataItem = (id: number | undefined) => {
  const { data } = useMetadata();

  if (id === undefined) {
    return undefined;
  }

  return data.find((metadata) => metadata.id === id);
};

export const useMetadataData = () => {
  const { data } = useMetadata();

  return data;
};

export const useSortedMetadata = () => {
  const result = useMetadata();

  const sortedData = useMemo(
    () => result.data.sort(sortByProp('name')),
    [result.data]
  );

  return {
    ...result,
    data: sortedData,
  };
};

export const useMetadata = () => {
  return useProviderSettings<MetadataModel>({
    path: PATH,
  });
};

export const useManageMetadata = (
  id: number | undefined,
  selectedSchema?: SelectedSchema
) => {
  const schema = useSelectedSchema<MetadataModel>(PATH, selectedSchema);

  if (selectedSchema && !schema) {
    throw new Error('A selected schema is required to manage metadata');
  }

  const manage = useManageProviderSettings<MetadataModel>(
    id,
    selectedSchema && schema
      ? {
          ...schema,
          name: schema.implementationName || '',
          enable: true,
        }
      : ({} as MetadataModel),
    PATH
  );

  return manage;
};

export const useDeleteMetadata = (id: number) => {
  const result = useDeleteProvider<MetadataModel>(id, PATH);

  return {
    ...result,
    deleteMetadata: result.deleteProvider,
  };
};

export const useMetadataSchema = (enabled: boolean = true) => {
  return useProviderSchema<MetadataModel>(PATH, enabled);
};
