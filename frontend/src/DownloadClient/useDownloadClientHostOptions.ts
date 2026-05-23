import { useMemo } from 'react';
import { useDownloadClientsData } from 'Settings/DownloadClients/DownloadClients/useDownloadClients';

export default function useDownloadClientHostOptions() {
  const downloadClients = useDownloadClientsData();

  return useMemo(() => {
    const hosts = downloadClients.reduce<Record<string, string[]>>(
      (acc, downloadClient) => {
        const name = downloadClient.name;
        const host = downloadClient.fields.find((field) => {
          return field.name === 'host';
        });

        if (host) {
          const hostValue = host.value as string;

          const group = (acc[hostValue] = acc[hostValue] || []);
          group.push(name);
        }

        return acc;
      },
      {}
    );

    return Object.keys(hosts).map((host) => {
      return {
        key: host,
        value: host,
        hint: `${hosts[host].join(', ')}`,
      };
    });
  }, [downloadClients]);
}
