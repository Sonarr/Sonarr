import ModelBase from 'App/ModelBase';
import Quality from './Quality';

export default interface QualityDefinitionModel extends ModelBase {
  quality: Quality;
  title: string;
  weight: number;
  minSize: number;
  maxSize: number;
  preferredSize: number;
}
