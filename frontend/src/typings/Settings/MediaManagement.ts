export default interface MediaManagement {
  autoUnmonitorPreviouslyDownloadedEpisodes: boolean;
  recycleBin: string;
  recycleBinCleanupDays: number;
  downloadPropersAndRepacks: string;
  createEmptySeriesFolders: boolean;
  deleteEmptyFolders: boolean;
  fileDate: string;
  rescanAfterRefresh: string;
  setPermissionsLinux: boolean;
  chmodFolder: string;
  chownGroup: string;
  episodeTitleRequired: string;
  skipFreeSpaceCheckWhenImporting: boolean;
  minimumFreeSpaceWhenImporting: number;
  copyUsingHardlinks: boolean;
  useScriptImport: boolean;
  scriptImportPath: string;
  importExtraFiles: boolean;
  extraFileExtensions: string;
  userRejectedExtensions: string;
  enableMediaInfo: boolean;
  seasonPackUpgrade: string;
  seasonPackUpgradeThreshold: number;
}
