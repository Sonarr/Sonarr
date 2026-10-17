import Language from 'Language/Language';
import Series from 'Series/Series';

interface Folder {
  language: Language;
  folder: string;
}

interface AddSeries extends Series {
  folder: string;
  folders: Folder[];
  isExcluded: boolean;
}

export default AddSeries;
