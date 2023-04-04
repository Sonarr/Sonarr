import { CustomFilter } from './AppState';

interface ClientSideCollectionAppState {
  totalItems: number;
  customFilters: CustomFilter[];
}

export default ClientSideCollectionAppState;
