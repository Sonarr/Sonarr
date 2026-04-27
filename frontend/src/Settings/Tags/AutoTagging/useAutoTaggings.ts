import { useCallback, useMemo } from 'react';
import ModelBase from 'App/ModelBase';
import { useProviderSchema } from 'Settings/useProviderSchema';
import {
  useDeleteProvider,
  useManageProviderSettings,
  useProviderSettings,
} from 'Settings/useProviderSettings';
import Field from 'typings/Field';
import { sortByProp } from 'Utilities/Array/sortByProp';
import translate from 'Utilities/String/translate';

export interface AutoTaggingSpecification {
  id: number;
  name: string;
  implementation: string;
  implementationName: string;
  negate: boolean;
  required: boolean;
  fields: Field[];
}

export interface AutoTagging extends ModelBase {
  name: string;
  removeTagsAutomatically: boolean;
  tags: number[];
  specifications: AutoTaggingSpecification[];
}

const PATH = '/autoTagging';

const DEFAULT_AUTO_TAGGING: AutoTagging = {
  id: 0,
  name: '',
  removeTagsAutomatically: false,
  tags: [],
  specifications: [],
};

export const useAutoTaggings = () => {
  return useProviderSettings<AutoTagging>({
    path: PATH,
    queryOptions: {
      refetchOnWindowFocus: false,
    },
  });
};

export const useSortedAutoTaggings = () => {
  const result = useAutoTaggings();

  const sortedData = useMemo(
    () => [...result.data].sort(sortByProp('name')),
    [result.data]
  );

  return { ...result, data: sortedData };
};

export const useAutoTagging = (id: number | undefined) => {
  const { data } = useAutoTaggings();

  if (id === undefined) {
    return undefined;
  }

  return data.find((at) => at.id === id);
};

export const useAutoTaggingsWithIds = (ids: number[]) => {
  const { data } = useAutoTaggings();

  return data.filter((at) => ids.includes(at.id));
};

export const useDeleteAutoTagging = (id: number) => {
  const result = useDeleteProvider<AutoTagging>(id, PATH);

  return {
    ...result,
    deleteAutoTagging: result.deleteProvider,
  };
};

export const useAutoTaggingSchema = (enabled: boolean = true) => {
  return useProviderSchema<AutoTaggingSpecification>(PATH, enabled);
};

function getNextSpecId(specifications: AutoTaggingSpecification[]) {
  return specifications.length > 0
    ? Math.max(...specifications.map((s) => s.id)) + 1
    : 1;
}

export const useManageAutoTagging = (
  id: number | undefined,
  cloneId: number | undefined
) => {
  const cloneSource = useAutoTagging(cloneId);

  if (cloneId && !cloneSource) {
    throw new Error(`AutoTagging with ID ${cloneId} not found`);
  }

  const defaultProvider = useMemo<AutoTagging>(() => {
    if (cloneId && cloneSource) {
      return {
        ...cloneSource,
        id: 0,
        name: translate('DefaultNameCopiedProfile', {
          name: cloneSource.name,
        }),
      };
    }

    return DEFAULT_AUTO_TAGGING;
  }, [cloneId, cloneSource]);

  const manage = useManageProviderSettings<AutoTagging>(
    id,
    defaultProvider,
    PATH
  );

  const specifications = useMemo(
    () =>
      manage.item.specifications.value.map((spec, i) => ({
        ...spec,
        id: spec.id ?? i + 1,
      })),
    [manage.item.specifications.value]
  );

  const saveSpecification = useCallback(
    (spec: AutoTaggingSpecification) => {
      if (spec.id > 0 && specifications.some((s) => s.id === spec.id)) {
        manage.updateValue(
          'specifications',
          specifications.map((s) => (s.id === spec.id ? spec : s))
        );
        return;
      }

      const newId = getNextSpecId(specifications);

      manage.updateValue('specifications', [
        ...specifications,
        { ...spec, id: newId },
      ]);
    },
    [specifications, manage]
  );

  const deleteSpecification = useCallback(
    (specId: number) => {
      manage.updateValue(
        'specifications',
        specifications.filter((s) => s.id !== specId)
      );
    },
    [specifications, manage]
  );

  const cloneSpecification = useCallback(
    (specId: number) => {
      const spec = specifications.find((s) => s.id === specId);

      if (!spec) {
        return;
      }

      const newId = getNextSpecId(specifications);

      manage.updateValue('specifications', [
        ...specifications,
        {
          ...spec,
          id: newId,
          name: translate('DefaultNameCopiedSpecification', {
            name: spec.name,
          }),
        },
      ]);
    },
    [specifications, manage]
  );

  return {
    ...manage,
    saveAutoTagging: manage.saveProvider,
    specifications,
    saveSpecification,
    deleteSpecification,
    cloneSpecification,
  };
};
