import ModelBase from 'App/ModelBase';
import Field from './Field';

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
