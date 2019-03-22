import migrateAddSeriesDefaults from './migrateAddSeriesDefaults';

export default function migrate(persistedState) {
  migrateAddSeriesDefaults(persistedState);
}
