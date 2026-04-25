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

export interface ExternalDecisionModel extends Provider {
  enable: boolean;
  decisionType: string;
  priority: number;
  tags: number[];
}

const PATH = '/externaldecision';

export const useExternalDecision = (id: number | undefined) => {
  const { data } = useExternalDecisions();

  if (id === undefined) {
    return undefined;
  }

  return data.find((schema) => schema.id === id);
};

export const useExternalDecisionsData = () => {
  const { data } = useExternalDecisions();

  return data;
};

export const useSortedExternalDecisions = () => {
  const { data } = useExternalDecisions();

  return useMemo(
    () =>
      [...data].sort(
        (a, b) => a.priority - b.priority || a.name.localeCompare(b.name)
      ),
    [data]
  );
};

export const useExternalDecisions = () => {
  return useProviderSettings<ExternalDecisionModel>({
    path: PATH,
  });
};

export const useManageExternalDecision = (
  id: number | undefined,
  selectedSchema?: SelectedSchema
) => {
  const schema = useSelectedSchema<ExternalDecisionModel>(PATH, selectedSchema);

  if (selectedSchema && !schema) {
    throw new Error(
      'A selected schema is required to manage an external decision'
    );
  }

  const manage = useManageProviderSettings<ExternalDecisionModel>(
    id,
    selectedSchema && schema
      ? {
          ...schema,
          name: schema.implementationName || '',
          enable: true,
          decisionType: 'rejection',
          priority: 25,
        }
      : ({} as ExternalDecisionModel),
    PATH
  );

  return manage;
};

export const useDeleteExternalDecision = (id: number) => {
  const result = useDeleteProvider<ExternalDecisionModel>(id, PATH);

  return {
    ...result,
    deleteExternalDecision: result.deleteProvider,
  };
};

export const useExternalDecisionSchema = (enabled: boolean = true) => {
  return useProviderSchema<ExternalDecisionModel>(PATH, enabled);
};
