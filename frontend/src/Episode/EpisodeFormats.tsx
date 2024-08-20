import React from 'react';
import Label from 'Components/Label';
import { kinds } from 'Helpers/Props';
import CustomFormat from 'typings/CustomFormat';

interface EpisodeFormatsProps {
  formats: CustomFormat[];
}

function EpisodeFormats({ formats }: EpisodeFormatsProps) {
  return (
    <div>
      {formats.map(({ id, name }) => (
        <Label key={id} kind={kinds.INFO}>
          {name}
        </Label>
      ))}
    </div>
  );
}

export default EpisodeFormats;
