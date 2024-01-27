export type ListSyncLevel =
  | 'disabled'
  | 'logOnly'
  | 'keepAndUnmonitor'
  | 'keepAndTag';

export default interface ImportListOptionsSettings {
  listSyncLevel: ListSyncLevel;
  listSyncTag: number;
}
