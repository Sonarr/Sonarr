import ModelBase from 'App/ModelBase';
import Field from './Field';

export type Protocol = 'torrent' | 'usenet' | 'unknown';

interface DownloadClient extends ModelBase {
  enable: boolean;
  protocol: Protocol;
  priority: number;
  removeCompletedDownloads: boolean;
  removeFailedDownloads: boolean;
  name: string;
  fields: Field[];
  implementationName: string;
  implementation: string;
  configContract: string;
  infoLink: string;
  tags: number[];
}

export default DownloadClient;
