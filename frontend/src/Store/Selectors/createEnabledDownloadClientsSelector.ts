import { createSelector } from 'reselect';
import { DownloadClientAppState } from 'App/State/SettingsAppState';
import DownloadProtocol from 'DownloadClient/DownloadProtocol';
import createSortedSectionSelector from 'Store/Selectors/createSortedSectionSelector';
import DownloadClient from 'typings/DownloadClient';
import sortByProp from 'Utilities/Array/sortByProp';

export default function createEnabledDownloadClientsSelector(
  protocol: DownloadProtocol
) {
  return createSelector(
    createSortedSectionSelector<DownloadClient>(
      'settings.downloadClients',
      sortByProp('name')
    ),
    (downloadClients: DownloadClientAppState) => {
      const { isFetching, isPopulated, error, items } = downloadClients;

      const clients = items.filter(
        (item) => item.protocol === protocol && item.enable
      );

      return { isFetching, isPopulated, error, items: clients };
    }
  );
}
