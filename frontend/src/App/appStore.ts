import { create } from 'zustand';
import { useShallow } from 'zustand/react/shallow';
import getQueryPath from 'Utilities/Fetch/getQueryPath';
import fetchJson from 'Utilities/requestAction';

function getDimensions(width: number, height: number) {
  const dimensions = {
    width,
    height,
    isExtraSmallScreen: width <= 480,
    isSmallScreen: width <= 768,
    isMediumScreen: width <= 992,
    isLargeScreen: width <= 1200,
  };

  return dimensions;
}

interface Dimensions {
  width: number;
  height: number;
  isExtraSmallScreen: boolean;
  isSmallScreen: boolean;
  isMediumScreen: boolean;
  isLargeScreen: boolean;
}

interface AppState {
  dimensions: Dimensions;
  version: string;
  prevVersion?: string;
  isUpdated: boolean;
  isConnected: boolean;
  isReconnecting: boolean;
  isDisconnected: boolean;
  isRestarting: boolean;
  isSidebarVisible: boolean;
}

// Variables for ping functionality
let abortPingServer: (() => void) | null = null;
let pingTimeout: ReturnType<typeof setTimeout> | null = null;

const useAppStore = create<AppState>()(() => {
  const dimensions = getDimensions(window.innerWidth, window.innerHeight);

  return {
    dimensions,
    version: window.Sonarr.version,
    isUpdated: false,
    isConnected: true,
    isReconnecting: false,
    isDisconnected: false,
    isRestarting: false,
    isSidebarVisible: !dimensions.isSmallScreen,
  };
});

export const useAppValues = <K extends keyof AppState>(...keys: K[]) => {
  return useAppStore(
    useShallow((state) => {
      return keys.reduce((acc, key) => {
        acc[key] = state[key];
        return acc;
      }, {} as Pick<AppState, K>);
    })
  );
};

export const useAppValue = <K extends keyof AppState>(key: K) => {
  return useAppStore(useShallow((state) => state[key]));
};

export const useAppDimensions = () => {
  return useAppStore(useShallow((state) => state.dimensions));
};

export const useAppDimension = <K extends keyof Dimensions>(key: K) => {
  return useAppStore(useShallow((state) => state.dimensions[key]));
};

export const getAppDimensions = () => {
  return useAppStore.getState().dimensions;
};

export const getAppValues = <K extends keyof AppState>(...keys: K[]) => {
  const state = useAppStore.getState();
  return keys.reduce((acc, key) => {
    acc[key] = state[key];
    return acc;
  }, {} as Pick<AppState, K>);
};

export const getAppValue = <K extends keyof AppState>(key: K) => {
  return useAppStore.getState()[key];
};

function pingServerAfterTimeout() {
  if (abortPingServer) {
    abortPingServer();
    abortPingServer = null;
  }

  if (pingTimeout) {
    clearTimeout(pingTimeout);
    pingTimeout = null;
  }

  pingTimeout = setTimeout(async () => {
    const { isRestarting, isConnected } = getAppValues(
      'isRestarting',
      'isConnected'
    );

    if (!isRestarting && isConnected) {
      return;
    }

    const abortController = new AbortController();
    abortPingServer = () => abortController.abort();

    try {
      await fetchJson({
        url: getQueryPath('/system/status'),
        method: 'GET',
        signal: abortController.signal,
        headers: {
          'X-Api-Key': window.Sonarr.apiKey,
          'X-Sonarr-Client': 'Sonarr',
        },
      });

      abortPingServer = null;
      pingTimeout = null;

      setAppValue({
        isRestarting: false,
      });
    } catch (error: unknown) {
      abortPingServer = null;
      pingTimeout = null;

      if ((error as { status?: number }).status === 401) {
        setAppValue({
          isRestarting: false,
        });
      } else if (!abortController.signal.aborted) {
        pingServerAfterTimeout();
      }
    }
  }, 5000);
}

export const saveDimensions = ({
  width,
  height,
}: {
  width: number;
  height: number;
}) => {
  const dimensions = getDimensions(width, height);
  useAppStore.setState({ dimensions });
};

export const setVersion = ({ version }: { version: string }) => {
  useAppStore.setState((state) => {
    const newState: Partial<AppState> = {
      version,
    };

    if (state.version !== version) {
      if (!state.prevVersion) {
        newState.prevVersion = state.version;
      }
      newState.isUpdated = true;
    }

    return newState;
  });
};

export const setIsSidebarVisible = ({
  isSidebarVisible,
}: {
  isSidebarVisible: boolean;
}) => {
  useAppStore.setState({ isSidebarVisible });
};

export const toggleIsSidebarVisible = () => {
  useAppStore.setState((state) => ({
    isSidebarVisible: !state.isSidebarVisible,
  }));
};

export const setAppValue = (payload: Partial<AppState>) => {
  useAppStore.setState(payload);
};

export const pingServer = () => {
  pingServerAfterTimeout();
};
