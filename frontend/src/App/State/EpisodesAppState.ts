import AppSectionState from 'App/State/AppSectionState';
import Column from 'Components/Table/Column';
import Episode from 'Episode/Episode';

interface EpisodesAppState extends AppSectionState<Episode> {
  columns: Column[];
}

export default EpisodesAppState;
