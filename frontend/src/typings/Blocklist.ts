import ModelBase from 'App/ModelBase';
import Language from 'Language/Language';
import { QualityModel } from 'Quality/Quality';
import CustomFormat from 'typings/CustomFormat';

interface Blocklist extends ModelBase {
  languages: Language[];
  quality: QualityModel;
  customFormats: CustomFormat[];
  title: string;
  date?: string;
  protocol: string;
  seriesId?: number;
}

export default Blocklist;
