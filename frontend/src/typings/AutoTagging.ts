import ModelBase from 'App/ModelBase';
import Field from './Field';

export interface AutoTaggingSpecification {
  id: number;
  name: string;
  implementation: string;
  implementationName: string;
  negate: boolean;
  required: boolean;
  fields: Field[];
}

interface AutoTagging extends ModelBase {
  name: string;
  removeTagsAutomatically: boolean;
  tags: number[];
  specifications: AutoTaggingSpecification[];
}

export default AutoTagging;
