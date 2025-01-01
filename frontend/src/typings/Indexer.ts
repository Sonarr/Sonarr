import DownloadProtocol from 'DownloadClient/DownloadProtocol';
import Provider from './Provider';

interface Indexer extends Provider {
  enableRss: boolean;
  enableAutomaticSearch: boolean;
  enableInteractiveSearch: boolean;
  supportsRss: boolean;
  supportsSearch: boolean;
  seasonSearchMaximumSingleEpisodeAge: number;
  protocol: DownloadProtocol;
  priority: number;
  downloadClientId: number;
  tags: number[];
}

export default Indexer;
