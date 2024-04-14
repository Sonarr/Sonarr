import ReleaseType from 'InteractiveImport/ReleaseType';
import translate from 'Utilities/String/translate';

export default function getReleaseTypeName(
  releaseType?: ReleaseType
): string | null {
  switch (releaseType) {
    case 'singleEpisode':
      return translate('SingleEpisode');
    case 'multiEpisode':
      return translate('MultiEpisode');
    case 'seasonPack':
      return translate('SeasonPack');
    default:
      return translate('Unknown');
  }
}
