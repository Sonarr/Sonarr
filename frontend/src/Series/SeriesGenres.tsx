import React from 'react';
import Label from 'Components/Label';
import Tooltip from 'Components/Tooltip/Tooltip';
import { kinds, sizes, tooltipPositions } from 'Helpers/Props';

interface SeriesGenresProps {
  className?: string;
  genres: string[];
}

function SeriesGenres({ className, genres }: SeriesGenresProps) {
  const [firstGenre, ...otherGenres] = genres;

  if (otherGenres.length) {
    return (
      <Tooltip
        anchor={<span className={className}>{firstGenre}</span>}
        tooltip={
          <div>
            {otherGenres.map((tag) => {
              return (
                <Label key={tag} kind={kinds.INFO} size={sizes.LARGE}>
                  {tag}
                </Label>
              );
            })}
          </div>
        }
        kind={kinds.INVERSE}
        position={tooltipPositions.TOP}
      />
    );
  }

  return <span className={className}>{firstGenre}</span>;
}

export default SeriesGenres;
