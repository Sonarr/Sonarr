import Quality from './Quality';

export default interface QualityDefinition {
  quality: Quality;
  title: string;
  weight: number;
  minSize: number;
  maxSize: number;
  preferredSize: number;
  id: number;
}
