import React, { useMemo } from 'react';
import * as episodeOrderTypes from 'Utilities/Series/episodeOrderTypes';
import translate from 'Utilities/String/translate';
import EnhancedSelectInput, {
  EnhancedSelectInputProps,
  EnhancedSelectInputValue,
} from './EnhancedSelectInput';

export interface EpisodeOrderSelectInputProps
  extends Omit<
    EnhancedSelectInputProps<EnhancedSelectInputValue<string>, string>,
    'values'
  > {
  includeNoChange?: boolean;
  includeNoChangeDisabled?: boolean;
  availableTypes?: string[];
}

function EpisodeOrderSelectInput(props: EpisodeOrderSelectInputProps) {
  const { includeNoChange = false, availableTypes, ...otherProps } = props;

  const values = useMemo(() => {
    let result = [
      {
        key: episodeOrderTypes.DEFAULT,
        value: translate('AiredOrder'),
      },
      {
        key: episodeOrderTypes.DVD,
        value: translate('DvdOrder'),
      },
      {
        key: episodeOrderTypes.ABSOLUTE,
        value: translate('AbsoluteOrder'),
      },
      {
        key: episodeOrderTypes.ALTERNATE,
        value: translate('AlternateOrder'),
      },
      {
        key: episodeOrderTypes.ALT_DVD,
        value: translate('AlternateDvdOrder'),
      },
      {
        key: episodeOrderTypes.REGIONAL,
        value: translate('RegionalOrder'),
      },
    ];

    // Filter to only show orderings available from TVDB for this series
    if (availableTypes && availableTypes.length > 0) {
      result = result.filter(
        (opt) =>
          opt.key === episodeOrderTypes.DEFAULT ||
          availableTypes.includes(opt.key)
      );
    }

    if (includeNoChange) {
      result.unshift({
        key: 'noChange',
        value: translate('NoChange'),
      });
    }

    return result;
  }, [includeNoChange, availableTypes]);

  return <EnhancedSelectInput {...otherProps} values={values} />;
}

export default EpisodeOrderSelectInput;
