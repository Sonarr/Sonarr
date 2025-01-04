import Provider from './Provider';

interface Notification extends Provider {
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

export default Notification;
