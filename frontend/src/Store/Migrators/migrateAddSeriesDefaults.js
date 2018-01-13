import { get } from 'lodash';
import monitorOptions from 'Utilities/Series/monitorOptions';

export default function migrateAddSeriesDefaults(persistedState) {
  const monitor = get(persistedState, 'addSeries.defaults.monitor');

  if (!monitor) {
    return;
  }

  if (!monitorOptions.find((option) => option.key === monitor)) {
    persistedState.addSeries.defaults.monitor = monitorOptions[0].key;
  }
}
