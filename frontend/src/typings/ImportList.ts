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

interface ImportList extends ModelBase {
  enable: boolean;
  enableAutomaticAdd: boolean;
  qualityProfileId: number;
  rootFolderPath: string;
  name: string;
  fields: Field[];
  implementationName: string;
  implementation: string;
  configContract: string;
  infoLink: string;
  tags: number[];
}

export default ImportList;
