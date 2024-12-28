export type SaveCallback = () => void;

export interface SettingsStateChange {
  isSaving: boolean;
  hasPendingChanges: boolean;
}

export type SetChildSave = (SaveCallback: SaveCallback) => void;

export type OnChildStateChange = (change: SettingsStateChange) => void;
