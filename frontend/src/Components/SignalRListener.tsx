import {
  HubConnection,
  HubConnectionBuilder,
  LogLevel,
} from '@microsoft/signalr';
import { useQueryClient } from '@tanstack/react-query';
import { useEffect, useRef } from 'react';
import { useDispatch } from 'react-redux';
import ModelBase from 'App/ModelBase';
import Command from 'Commands/Command';
import { setAppValue, setVersion } from 'Store/Actions/appActions';
import { removeItem, updateItem } from 'Store/Actions/baseActions';
import {
  fetchCommands,
  finishCommand,
  updateCommand,
} from 'Store/Actions/commandActions';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import { fetchSeries } from 'Store/Actions/seriesActions';
import { fetchQualityDefinitions } from 'Store/Actions/settingsActions';
import { fetchTagDetails, fetchTags } from 'Store/Actions/tagActions';
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
  const dispatch = useDispatch();

  const connection = useRef<HubConnection | null>(null);

  const handleStartFail = useRef((error: unknown) => {
    console.error('[signalR] failed to connect');
    console.error(error);

    dispatch(
      setAppValue({
        isConnected: false,
        isReconnecting: false,
        isDisconnected: false,
        isRestarting: false,
      })
    );
  });

  const handleStart = useRef(() => {
    console.debug('[signalR] connected');

    dispatch(
      setAppValue({
        isConnected: true,
        isReconnecting: false,
        isDisconnected: false,
        isRestarting: false,
      })
    );
  });

  const handleReconnecting = useRef(() => {
    dispatch(setAppValue({ isReconnecting: true }));
  });

  const handleReconnected = useRef(() => {
    dispatch(
      setAppValue({
        isConnected: true,
        isReconnecting: false,
        isDisconnected: false,
        isRestarting: false,
      })
    );

    // Repopulate the page (if a repopulator is set) to ensure things
    // are in sync after reconnecting.
    dispatch(fetchSeries());
    dispatch(fetchCommands());
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
        dispatch(fetchCommands());
        return;
      }

      const resource = body.resource as Command;
      const status = resource.status;

      // Both successful and failed commands need to be
      // completed, otherwise they spin until they time out.

      if (status === 'completed' || status === 'failed') {
        dispatch(finishCommand(resource));
      } else {
        dispatch(updateCommand(resource));
      }

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
      if (body.action === 'updated') {
        dispatch(
          updateItem({
            section: 'episodes',
            updateOnly: true,
            ...body.resource,
          })
        );
      }

      return;
    }

    if (name === 'episodefile') {
      const section = 'episodeFiles';

      if (body.action === 'updated') {
        dispatch(updateItem({ section, ...body.resource }));

        // Repopulate the page to handle recently imported file
        repopulatePage('episodeFileUpdated');
      } else if (body.action === 'deleted') {
        dispatch(removeItem({ section, id: body.resource.id }));

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
      const section = 'settings.indexers';

      if (body.action === 'created' || body.action === 'updated') {
        dispatch(updateItem({ section, ...body.resource }));
      } else if (body.action === 'deleted') {
        dispatch(removeItem({ section, id: body.resource.id }));
      }

      return;
    }

    if (name === 'metadata') {
      const section = 'settings.metadata';

      if (body.action === 'updated') {
        dispatch(updateItem({ section, ...body.resource }));
      }

      return;
    }

    if (name === 'notification') {
      const section = 'settings.notifications';

      if (body.action === 'created' || body.action === 'updated') {
        dispatch(updateItem({ section, ...body.resource }));
      } else if (body.action === 'deleted') {
        dispatch(removeItem({ section, id: body.resource.id }));
      }

      return;
    }

    if (name === 'qualitydefinition') {
      dispatch(fetchQualityDefinitions());
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
      dispatch(fetchRootFolders());

      return;
    }

    if (name === 'series') {
      if (body.action === 'updated') {
        dispatch(updateItem({ section: 'series', ...body.resource }));

        repopulatePage('seriesUpdated');
      } else if (body.action === 'deleted') {
        dispatch(removeItem({ section: 'series', id: body.resource.id }));
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
      if (body.action === 'sync') {
        dispatch(fetchTags());
        dispatch(fetchTagDetails());
      }

      return;
    }

    if (name === 'version') {
      dispatch(setVersion({ version: body.version }));
      return;
    }

    if (name === 'wanted/cutoff') {
      if (body.action === 'updated') {
        dispatch(
          updateItem({
            section: 'wanted.cutoffUnmet',
            updateOnly: true,
            ...body.resource,
          })
        );
      }

      return;
    }

    if (name === 'wanted/missing') {
      if (body.action === 'updated') {
        dispatch(
          updateItem({
            section: 'wanted.missing',
            updateOnly: true,
            ...body.resource,
          })
        );
      }

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
            dispatch(setAppValue({ isDisconnected: true }));
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
