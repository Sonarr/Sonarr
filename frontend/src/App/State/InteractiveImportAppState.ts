import AppSectionState from 'App/State/AppSectionState';
import RecentFolder from 'InteractiveImport/Folder/RecentFolder';
import ImportMode from 'InteractiveImport/ImportMode';
import InteractiveImport from 'InteractiveImport/InteractiveImport';

interface InteractiveImportAppState extends AppSectionState<InteractiveImport> {
  originalItems: InteractiveImport[];
  importMode: ImportMode;
  recentFolders: RecentFolder[];
}

export default InteractiveImportAppState;
