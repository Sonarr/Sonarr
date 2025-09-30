import ModelBase from 'App/ModelBase';
import DownloadProtocol from 'DownloadClient/DownloadProtocol';
import Language from 'Language/Language';
import { QualityModel } from 'Quality/Quality';
import CustomFormat from 'typings/CustomFormat';

interface Blocklist extends ModelBase {
  languages: Language[];
  quality: QualityModel;
  customFormats: CustomFormat[];
  title: string;
  date?: string;
  protocol: DownloadProtocol;
  sourceTitle: string;
  seriesId?: number;
  indexer?: string;
  message?: string;
  source?: string;
}

export default Blocklist;
