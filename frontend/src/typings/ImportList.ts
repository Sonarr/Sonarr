import Provider from './Provider';

interface ImportList extends Provider {
  enable: boolean;
  enableAutomaticAdd: boolean;
  qualityProfileId: number;
  rootFolderPath: string;
  tags: number[];
}

export default ImportList;
