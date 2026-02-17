import {
  HubConnection,
  HubConnectionBuilder,
  LogLevel,
} from '@microsoft/signalr';
import { QueryKey, useQueryClient } from '@tanstack/react-query';
import { useEffect, useRef } from 'react';
import { useDispatch } from 'react-redux';
import { setAppValue, setVersion } from 'App/appStore';
import ModelBase from 'App/ModelBase';
import Command from 'Commands/Command';
import { useUpdateCommand } from 'Commands/useCommands';
import Episode from 'Episode/Episode';
import { EpisodeFile } from 'EpisodeFile/EpisodeFile';
import { PagedQueryResponse } from 'Helpers/Hooks/usePagedApiQuery';
import Series from 'Series/Series';
import { IndexerModel } from 'Settings/Indexers/useIndexers';
import { NotificationModel } from 'Settings/Notifications/useConnections';
import { removeItem, updateItem } from 'Store/Actions/baseActions';
import { repopulatePage } from 'Utilities/pagePopulator';
import SignalRLogger from 'Utilities/SignalRLogger';

type SignalRAction = 'sync' | 'created' | 'updated' | 'deleted';

interface SignalRMessage {
  name: string;
  body: {
    action: SignalRAction;
    resource: ModelBase;
    version: string;
  };
  version: number | undefined;
}

function SignalRListener() {
  const queryClient = useQueryClient();
  const updateCommand = useUpdateCommand();
  const dispatch = useDispatch();

  const connection = useRef<HubConnection | null>(null);

  const handleStartFail = useRef((error: unknown) => {
    console.error('[signalR] failed to connect');
    console.error(error);

    setAppValue({
      isConnected: false,
      isReconnecting: false,
      isDisconnected: false,
      isRestarting: false,
    });
  });

  const handleStart = useRef(() => {
    console.debug('[signalR] connected');

    setAppValue({
      isConnected: true,
      isReconnecting: false,
      isDisconnected: false,
      isRestarting: false,
    });
  });

  const handleReconnecting = useRef(() => {
    setAppValue({ isReconnecting: true });
  });

  const handleReconnected = useRef(() => {
    setAppValue({
      isConnected: true,
      isReconnecting: false,
      isDisconnected: false,
      isRestarting: false,
    });

    // Repopulate the page (if a repopulator is set) to ensure things
    // are in sync after reconnecting.
    queryClient.invalidateQueries({ queryKey: ['/series'] });

    queryClient.invalidateQueries({ queryKey: ['/command'] });

    repopulatePage();
  });

  const handleClose = useRef(() => {
    console.debug('[signalR] connection closed');
  });

  const handleReceiveMessage = useRef((message: SignalRMessage) => {
    console.debug(
      `[signalR] received ${message.name}${
        message.version ? ` v${message.version}` : ''
      }`,
      message.body
    );

    const { name, body, version = 0 } = message;

    if (name === 'calendar') {
      if (body.action === 'updated') {
        dispatch(
          updateItem({
            section: 'calendar',
            updateOnly: true,
            ...body.resource,
          })
        );
        return;
      }
    }

    if (name === 'command') {
      if (body.action === 'sync') {
        queryClient.invalidateQueries({ queryKey: ['/command'] });
        return;
      }

      const resource = body.resource as Command;

      updateCommand(resource);

      return;
    }

    if (name === 'downloadclient') {
      const section = 'settings.downloadClients';

      if (body.action === 'created' || body.action === 'updated') {
        dispatch(updateItem({ section, ...body.resource }));
      } else if (body.action === 'deleted') {
        dispatch(removeItem({ section, id: body.resource.id }));
      }

      return;
    }

    if (name === 'episode') {
      if (version < 5) {
        return;
      }

      if (body.action === 'updated') {
        const updatedItem = body.resource as Episode;

        updateQueryClientItem(
          queryClient,
          ['/episode'],
          updatedItem,
          false // Don't add the episode to the list if it doesn't exist. Episodes should already be in the list since they are included in the series details.
        );
      }

      return;
    }

    if (name === 'episodefile') {
      if (version < 5) {
        return;
      }

      if (body.action === 'updated') {
        const updatedItem = body.resource as EpisodeFile;

        updateQueryClientItem(
          queryClient,
          ['/episodeFile'],
          updatedItem,
          true // Add the episode file to the list if it doesn't exist. This can happen when an episode file is imported and wasn't previously in the list of episode files.
        );

        // Repopulate the page to handle recently imported file
        repopulatePage('episodeFileUpdated');
      } else if (body.action === 'deleted') {
        const id = body.resource.id;

        removeQueryClientItem(queryClient, ['/episodeFile'], id);
        repopulatePage('episodeFileDeleted');
      }

      return;
    }

    if (name === 'health') {
      if (version < 5) {
        return;
      }

      queryClient.invalidateQueries({ queryKey: ['/health'] });
      return;
    }

    if (name === 'importlist') {
      const section = 'settings.importLists';

      if (body.action === 'created' || body.action === 'updated') {
        dispatch(updateItem({ section, ...body.resource }));
      } else if (body.action === 'deleted') {
        dispatch(removeItem({ section, id: body.resource.id }));
      }

      return;
    }

    if (name === 'indexer') {
      const updatedItem = body.resource as IndexerModel;

      if (body.action === 'created' || body.action === 'updated') {
        updateQueryClientItem(queryClient, ['/indexer'], updatedItem, true);
      } else if (body.action === 'deleted') {
        removeQueryClientItem(queryClient, ['/indexer'], body.resource.id);
      }

      return;
    }

    if (name === 'metadata') {
      const updatedItem = body.resource as ModelBase;

      if (body.action === 'updated') {
        updateQueryClientItem(queryClient, ['/metadata'], updatedItem, false);
      }

      return;
    }

    if (name === 'connection') {
      const updatedItem = body.resource as NotificationModel;

      if (body.action === 'created' || body.action === 'updated') {
        updateQueryClientItem(
          queryClient,
          ['/connection'],
          updatedItem,
          body.action === 'created' // Only add the connection to the list if it was created. If it was updated and it doesn't exist in the list, it likely means the connection is disabled and shouldn't be shown in the list.
        );
      } else if (body.action === 'deleted') {
        removeQueryClientItem(queryClient, ['/connection'], body.resource.id);
      }

      return;
    }

    if (name === 'qualitydefinition') {
      if (version < 5) {
        return;
      }

      queryClient.invalidateQueries({ queryKey: ['/qualitydefinition'] });
      return;
    }

    if (name === 'queue') {
      if (version < 5) {
        return;
      }

      queryClient.invalidateQueries({ queryKey: ['/queue'] });
      return;
    }

    if (name === 'queue/details') {
      if (version < 5) {
        return;
      }

      queryClient.invalidateQueries({ queryKey: ['/queue/details'] });
      return;
    }

    if (name === 'queue/status') {
      if (version < 5) {
        return;
      }

      const statusDetails = queryClient.getQueriesData({
        queryKey: ['/queue/status'],
      });

      statusDetails.forEach(([queryKey]) => {
        queryClient.setQueryData(queryKey, () => body.resource);
      });

      return;
    }

    if (name === 'rootfolder') {
      if (version < 5) {
        return;
      }

      queryClient.invalidateQueries({ queryKey: ['/rootFolder'] });

      return;
    }

    if (name === 'series') {
      if (version < 5) {
        return;
      }

      if (body.action === 'updated') {
        const updatedItem = body.resource as Series;

        updateQueryClientItem(
          queryClient,
          ['/series'],
          updatedItem,
          false // Don't add the series to the list if it doesn't exist. Series should already be in the list since they are included in the calendar and series details.
        );

        repopulatePage('seriesUpdated');
      } else if (body.action === 'deleted') {
        removeQueryClientItem(queryClient, ['/series'], body.resource.id);
      }

      return;
    }

    if (name === 'system/task') {
      if (version < 5) {
        return;
      }

      queryClient.invalidateQueries({ queryKey: ['/system/task'] });
      return;
    }

    if (name === 'tag') {
      if (version < 5 || body.action !== 'sync') {
        return;
      }

      queryClient.invalidateQueries({ queryKey: ['/tag'] });
      queryClient.invalidateQueries({ queryKey: ['/tag/detail'] });

      return;
    }

    if (name === 'version') {
      setVersion({ version: body.version });
      return;
    }

    if (name === 'wanted/cutoff') {
      if (version < 5 || body.action !== 'updated') {
        return;
      }

      updatePagedItem<Episode>(
        queryClient,
        ['/wanted/cutoff'],
        body.resource as Episode
      );

      return;
    }

    if (name === 'wanted/missing') {
      if (version < 5 || body.action !== 'updated') {
        return;
      }

      updatePagedItem<Episode>(
        queryClient,
        ['/wanted/missing'],
        body.resource as Episode
      );

      return;
    }

    console.error(`signalR: Unable to find handler for ${name}`);
  });

  useEffect(() => {
    console.log('[signalR] starting');

    const url = `${window.Sonarr.urlBase}/signalr/messages`;

    connection.current = new HubConnectionBuilder()
      .configureLogging(new SignalRLogger(LogLevel.Information))
      .withUrl(
        `${url}?access_token=${encodeURIComponent(window.Sonarr.apiKey)}`
      )
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          if (retryContext.elapsedMilliseconds > 180000) {
            setAppValue({ isDisconnected: true });
          }
          return Math.min(retryContext.previousRetryCount, 10) * 1000;
        },
      })
      .build();

    connection.current.onreconnecting(handleReconnecting.current);
    connection.current.onreconnected(handleReconnected.current);
    connection.current.onclose(handleClose.current);

    connection.current.on('receiveMessage', handleReceiveMessage.current);

    connection.current
      .start()
      .then(handleStart.current, handleStartFail.current);

    return () => {
      connection.current?.stop();
      connection.current = null;
    };
  }, [dispatch]);

  return null;
}

export default SignalRListener;

const updatePagedItem = <T extends ModelBase>(
  queryClient: ReturnType<typeof useQueryClient>,
  queryKey: QueryKey,
  updatedItem: T
) => {
  queryClient.setQueriesData(
    { queryKey },
    (oldData: PagedQueryResponse<T> | undefined) => {
      if (!oldData) {
        return oldData;
      }

      const itemIndex = oldData.records.findIndex(
        (item) => item.id === updatedItem.id
      );

      if (itemIndex === -1) {
        return oldData;
      }

      return {
        ...oldData,
        records: oldData.records.map((item) => {
          if (item.id === updatedItem.id) {
            return updatedItem;
          }

          return item;
        }),
      };
    }
  );
};

const updateQueryClientItem = <T extends ModelBase>(
  queryClient: ReturnType<typeof useQueryClient>,
  queryKey: QueryKey,
  updatedItem: T,
  addMissing: boolean
) => {
  queryClient.setQueriesData({ queryKey }, (oldData: T[] | undefined) => {
    if (!oldData) {
      return oldData;
    }

    const itemIndex = oldData.findIndex((item) => item.id === updatedItem.id);

    if (itemIndex === -1 && addMissing) {
      return [...oldData, updatedItem];
    }

    return oldData.map((item) => {
      if (item.id === updatedItem.id) {
        return updatedItem;
      }

      return item;
    });
  });
};

const removeQueryClientItem = <T extends ModelBase>(
  queryClient: ReturnType<typeof useQueryClient>,
  queryKey: QueryKey,
  id: T['id']
) => {
  queryClient.setQueriesData({ queryKey }, (oldData: T[] | undefined) => {
    if (!oldData) {
      return oldData;
    }

    const itemIndex = oldData.findIndex((item) => item.id === id);

    if (itemIndex === -1) {
      return oldData;
    }

    return oldData.filter((item) => item.id !== id);
  });
};
