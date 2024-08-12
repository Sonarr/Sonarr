import React from 'react';
import Label from 'Components/Label';
import Popover from 'Components/Tooltip/Popover';
import { kinds, tooltipPositions } from 'Helpers/Props';
import Language from 'Language/Language';
import translate from 'Utilities/String/translate';

interface EpisodeLanguagesProps {
  className?: string;
  languages: Language[];
  isCutoffNotMet?: boolean;
}

function EpisodeLanguages(props: EpisodeLanguagesProps) {
  const { className, languages, isCutoffNotMet = true } = props;

  // TODO: Typescript - Remove once everything is converted
  if (!languages || languages.length === 0) {
    return null;
  }

  if (languages.length === 1) {
    return (
      <Label
        className={className}
        kind={isCutoffNotMet ? kinds.INVERSE : kinds.DEFAULT}
      >
        {languages[0].name}
      </Label>
    );
  }

  return (
    <Popover
      className={className}
      anchor={
        <Label
          className={className}
          kind={isCutoffNotMet ? kinds.INVERSE : kinds.DEFAULT}
        >
          {translate('MultiLanguages')}
        </Label>
      }
      title={translate('Languages')}
      body={
        <ul>
          {languages.map((language) => (
            <li key={language.id}>{language.name}</li>
          ))}
        </ul>
      }
      position={tooltipPositions.LEFT}
    />
  );
}

export default EpisodeLanguages;
