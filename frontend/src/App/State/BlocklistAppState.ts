import Blocklist from 'typings/Blocklist';
import AppSectionState, { AppSectionFilterState } from './AppSectionState';

interface BlocklistAppState
  extends AppSectionState<Blocklist>,
    AppSectionFilterState<Blocklist> {}

export default BlocklistAppState;
