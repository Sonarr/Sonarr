import ModelBase from 'App/ModelBase';

export interface Field {
  order: number;
  name: string;
  label: string;
  value: boolean | number | string;
  type: string;
  advanced: boolean;
  privacy: string;
}

interface DownloadClient extends ModelBase {
  enable: boolean;
  protocol: string;
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
