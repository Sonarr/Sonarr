import { createSelector } from 'reselect';
import RootFolderAppState from 'App/State/RootFolderAppState';
import createSortedSectionSelector from 'Store/Selectors/createSortedSectionSelector';
import RootFolder from 'typings/RootFolder';

export default function createRootFoldersSelector() {
  return createSelector(
    createSortedSectionSelector('rootFolders', (a: RootFolder, b: RootFolder) =>
      a.path.localeCompare(b.path)
    ),
    (rootFolders: RootFolderAppState) => rootFolders
  );
}
