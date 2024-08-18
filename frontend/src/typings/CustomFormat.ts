import ModelBase from 'App/ModelBase';

export interface QualityProfileFormatItem {
  format: number;
  name: string;
  score: number;
}

interface CustomFormat extends ModelBase {
  name: string;
  includeCustomFormatWhenRenaming: boolean;
}

export default CustomFormat;
