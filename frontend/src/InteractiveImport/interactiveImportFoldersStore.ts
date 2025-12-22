import moment from 'moment';
import { createPersist } from 'Helpers/createPersist';
import sortByProp from 'Utilities/Array/sortByProp';

const MAXIMUM_RECENT_FOLDERS = 10;

interface RecentFolder {
  folder: string;
  lastUsed: string;
}

interface FavoriteFolder {
  folder: string;
}

interface InteractiveImportFoldersState {
  recentFolders: RecentFolder[];
  favoriteFolders: FavoriteFolder[];
}

const store = createPersist<InteractiveImportFoldersState>(
  'interactive_import_folders',
  () => ({
    recentFolders: [],
    favoriteFolders: [],
  })
);

export const useInteractiveImportFolders = () => {
  return store((state) => state);
};

export const useRecentFolders = () => {
  return store((state) => state.recentFolders);
};

export const useFavoriteFolders = () => {
  return store((state) => state.favoriteFolders);
};

export const addRecentFolder = (folder: string) => {
  store.setState((state) => {
    const recentFolder: RecentFolder = {
      folder,
      lastUsed: moment().toISOString(),
    };
    const recentFolders = [...state.recentFolders];
    const index = recentFolders.findIndex((r) => r.folder === folder);

    if (index > -1) {
      recentFolders.splice(index, 1);
    }

    recentFolders.push(recentFolder);

    const sliceIndex = Math.max(
      recentFolders.length - MAXIMUM_RECENT_FOLDERS,
      0
    );

    return {
      ...state,
      recentFolders: recentFolders.slice(sliceIndex),
    };
  });
};

export const removeRecentFolder = (folder: string) => {
  store.setState((state) => {
    const recentFolders = [...state.recentFolders];
    const index = recentFolders.findIndex((r) => r.folder === folder);

    if (index > -1) {
      recentFolders.splice(index, 1);
    }

    return {
      ...state,
      recentFolders,
    };
  });
};

export const addFavoriteFolder = (folder: string) => {
  store.setState((state) => {
    const favoriteFolder: FavoriteFolder = { folder };
    const favoriteFolders = [...state.favoriteFolders, favoriteFolder].sort(
      sortByProp('folder')
    );

    return {
      ...state,
      favoriteFolders,
    };
  });
};

export const removeFavoriteFolder = (folder: string) => {
  store.setState((state) => {
    const favoriteFolders = state.favoriteFolders.filter(
      (item) => item.folder !== folder
    );

    return {
      ...state,
      favoriteFolders,
    };
  });
};

export const getInteractiveImportFolders = () => {
  return store.getState();
};

export const getRecentFolders = () => {
  return store.getState().recentFolders;
};

export const getFavoriteFolders = () => {
  return store.getState().favoriteFolders;
};
