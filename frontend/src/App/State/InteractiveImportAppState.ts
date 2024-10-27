import AppSectionState from 'App/State/AppSectionState';
import ImportMode from 'InteractiveImport/ImportMode';
import InteractiveImport from 'InteractiveImport/InteractiveImport';

interface FavoriteFolder {
  folder: string;
}

interface RecentFolder {
  folder: string;
  lastUsed: string;
}

interface InteractiveImportAppState extends AppSectionState<InteractiveImport> {
  originalItems: InteractiveImport[];
  importMode: ImportMode;
  favoriteFolders: FavoriteFolder[];
  recentFolders: RecentFolder[];
}

export default InteractiveImportAppState;
