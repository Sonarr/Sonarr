import ModelBase from 'App/ModelBase';
import Field from './Field';

interface Indexer extends ModelBase {
  enableRss: boolean;
  enableAutomaticSearch: boolean;
  enableInteractiveSearch: boolean;
  protocol: string;
  priority: number;
  name: string;
  fields: Field[];
  implementationName: string;
  implementation: string;
  configContract: string;
  infoLink: string;
  tags: number[];
}

export default Indexer;
