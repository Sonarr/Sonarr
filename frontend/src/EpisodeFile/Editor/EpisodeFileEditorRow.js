import PropTypes from 'prop-types';
import React from 'react';
import padNumber from 'Utilities/Number/padNumber';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import TableRow from 'Components/Table/TableRow';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import EpisodeLanguage from 'Episode/EpisodeLanguage';
import EpisodeQuality from 'Episode/EpisodeQuality';
import styles from './EpisodeFileEditorRow';

function EpisodeFileEditorRow(props) {
  const {
    id,
    seriesType,
    seasonNumber,
    episodeNumber,
    absoluteEpisodeNumber,
    relativePath,
    airDateUtc,
    language,
    quality,
    qualityCutoffNotMet,
    languageCutoffNotMet,
    isSelected,
    onSelectedChange
  } = props;

  return (
    <TableRow>
      <TableSelectCell
        id={id}
        isSelected={isSelected}
        onSelectedChange={onSelectedChange}
      />

      <TableRowCell>
        {seasonNumber}x{padNumber(episodeNumber, 2)}

        {
          seriesType === 'anime' && !!absoluteEpisodeNumber &&
            <span className={styles.absoluteEpisodeNumber}>
              ({absoluteEpisodeNumber})
            </span>
        }
      </TableRowCell>

      <TableRowCell>
        {relativePath}
      </TableRowCell>

      <RelativeDateCellConnector
        date={airDateUtc}
      />

      <TableRowCell>
        <EpisodeLanguage
          language={language}
          isCutoffNotMet={languageCutoffNotMet}
        />
      </TableRowCell>

      <TableRowCell>
        <EpisodeQuality
          quality={quality}
          isCutoffNotMet={qualityCutoffNotMet}
        />
      </TableRowCell>
    </TableRow>
  );
}

EpisodeFileEditorRow.propTypes = {
  id: PropTypes.number.isRequired,
  seriesType: PropTypes.string.isRequired,
  seasonNumber: PropTypes.number.isRequired,
  episodeNumber: PropTypes.number.isRequired,
  absoluteEpisodeNumber: PropTypes.number,
  relativePath: PropTypes.string.isRequired,
  airDateUtc: PropTypes.string.isRequired,
  language: PropTypes.object.isRequired,
  quality: PropTypes.object.isRequired,
  qualityCutoffNotMet: PropTypes.bool.isRequired,
  languageCutoffNotMet: PropTypes.bool.isRequired,
  isSelected: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired
};

export default EpisodeFileEditorRow;
