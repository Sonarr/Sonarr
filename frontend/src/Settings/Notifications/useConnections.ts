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

export interface NotificationModel extends Provider {
  enable: boolean;
  onGrab: boolean;
  onDownload: boolean;
  onUpgrade: boolean;
  onImportComplete: boolean;
  onRename: boolean;
  onSeriesAdd: boolean;
  onSeriesDelete: boolean;
  onEpisodeFileDelete: boolean;
  onEpisodeFileDeleteForUpgrade: boolean;
  onHealthIssue: boolean;
  includeHealthWarnings: boolean;
  onHealthRestored: boolean;
  onApplicationUpdate: boolean;
  onManualInteractionRequired: boolean;
  supportsOnGrab: boolean;
  supportsOnDownload: boolean;
  supportsOnUpgrade: boolean;
  supportsOnImportComplete: boolean;
  supportsOnRename: boolean;
  supportsOnSeriesAdd: boolean;
  supportsOnSeriesDelete: boolean;
  supportsOnEpisodeFileDelete: boolean;
  supportsOnEpisodeFileDeleteForUpgrade: boolean;
  supportsOnHealthIssue: boolean;
  supportsOnHealthRestored: boolean;
  supportsOnApplicationUpdate: boolean;
  supportsOnManualInteractionRequired: boolean;
  tags: number[];
}

const PATH = '/connection';

export const useConnectionsWithIds = (ids: number[]) => {
  const allNotifications = useConnectionsData();

  return allNotifications.filter((notification) =>
    ids.includes(notification.id)
  );
};

export const useConnection = (id: number | undefined) => {
  const { data } = useConnections();

  if (id === undefined) {
    return undefined;
  }

  return data.find((notification) => notification.id === id);
};

export const useConnectionsData = () => {
  const { data } = useConnections();

  return data;
};

export const useSortedConnections = () => {
  const { data } = useConnections();

  return useMemo(() => data.sort(sortByProp('name')), [data]);
};

export const useConnections = () => {
  return useProviderSettings<NotificationModel>({
    path: PATH,
  });
};

export const useManageConnection = (
  id: number | undefined,
  selectedSchema?: SelectedSchema
) => {
  const schema = useSelectedSchema<NotificationModel>(PATH, selectedSchema);

  if (selectedSchema && !schema) {
    throw new Error('A selected schema is required to manage a notification');
  }

  const manage = useManageProviderSettings<NotificationModel>(
    id,
    selectedSchema && schema
      ? {
          ...schema,
          name: schema.implementationName || '',
          onGrab: schema.supportsOnGrab || false,
          onDownload: schema.supportsOnDownload || false,
          onUpgrade: schema.supportsOnUpgrade || false,
          onImportComplete: schema.supportsOnImportComplete || false,
          onRename: schema.supportsOnRename || false,
          onSeriesAdd: schema.supportsOnSeriesAdd || false,
          onSeriesDelete: schema.supportsOnSeriesDelete || false,
          onEpisodeFileDelete: schema.supportsOnEpisodeFileDelete || false,
          onEpisodeFileDeleteForUpgrade:
            schema.supportsOnEpisodeFileDeleteForUpgrade || false,
          onApplicationUpdate: schema.supportsOnApplicationUpdate || false,
          onManualInteractionRequired:
            schema.supportsOnManualInteractionRequired || false,
        }
      : ({} as NotificationModel),
    PATH
  );

  return manage;
};

export const useDeleteConnection = (id: number) => {
  const result = useDeleteProvider<NotificationModel>(id, PATH);

  return {
    ...result,
    deleteConnection: result.deleteProvider,
  };
};

export const useConnectionSchema = (enabled: boolean = true) => {
  return useProviderSchema<NotificationModel>(PATH, enabled);
};
