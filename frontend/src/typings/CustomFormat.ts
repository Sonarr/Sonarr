import ModelBase from 'App/ModelBase';
import CustomFormatSpecification from './CustomFormatSpecification';

export interface QualityProfileFormatItem {
  format: number;
  name: string;
  score: number;
}

interface CustomFormat extends ModelBase {
  name: string;
  includeCustomFormatWhenRenaming: boolean;
  specifications: CustomFormatSpecification[];
}

export default CustomFormat;
