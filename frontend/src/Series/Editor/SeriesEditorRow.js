import PropTypes from 'prop-types';
import React, { Component } from 'react';
import CheckInput from 'Components/Form/CheckInput';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import TableRow from 'Components/Table/TableRow';
import TagListConnector from 'Components/TagListConnector';
import SeriesStatusCell from 'Series/Index/Table/SeriesStatusCell';
import SeriesTitleLink from 'Series/SeriesTitleLink';
import formatBytes from 'Utilities/Number/formatBytes';
import titleCase from 'Utilities/String/titleCase';
import styles from './SeriesEditorRow.css';

class SeriesEditorRow extends Component {

  //
  // Listeners

  onSeasonFolderChange = () => {
    // Mock handler to satisfy `onChange` being required for `CheckInput`.
    //
  };

  //
  // Render

  render() {
    const {
      id,
      monitored,
      status,
      title,
      titleSlug,
      seriesType,
      qualityProfile,
      languageProfile,
      path,
      tags,
      seasonFolder,
      statistics = {},
      columns,
      isSelected,
      onSelectedChange
    } = this.props;

    return (
      <TableRow>
        <TableSelectCell
          id={id}
          isSelected={isSelected}
          onSelectedChange={onSelectedChange}
        />

        {
          columns.map((column) => {
            const {
              name,
              isVisible
            } = column;

            if (!isVisible) {
              return null;
            }

            if (name === 'status') {
              return (
                <SeriesStatusCell
                  key={name}
                  monitored={monitored}
                  status={status}
                />
              );
            }

            if (name === 'sortTitle') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.title}
                >
                  <SeriesTitleLink

                    titleSlug={titleSlug}
                    title={title}
                  />
                </TableRowCell>
              );
            }

            if (name === 'qualityProfileId') {
              return (
                <TableRowCell key={name}>
                  {qualityProfile.name}
                </TableRowCell>
              );
            }

            if (name === 'languageProfileId') {
              return (
                <TableRowCell key={name}>
                  {languageProfile.name}
                </TableRowCell>
              );
            }

            if (name === 'seriesType') {
              return (
                <TableRowCell key={name}>
                  {titleCase(seriesType)}
                </TableRowCell>
              );
            }

            if (name === 'seasonFolder') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.seasonFolder}
                >
                  <CheckInput
                    name="seasonFolder"
                    value={seasonFolder}
                    isDisabled={true}
                    onChange={this.onSeasonFolderChange}
                  />
                </TableRowCell>
              );
            }

            if (name === 'path') {
              return (
                <TableRowCell key={name}>
                  {path}
                </TableRowCell>
              );
            }

            if (name === 'sizeOnDisk') {
              return (
                <TableRowCell key={name}>
                  {formatBytes(statistics.sizeOnDisk)}
                </TableRowCell>
              );
            }

            if (name === 'tags') {
              return (
                <TableRowCell key={name}>
                  <TagListConnector
                    tags={tags}
                  />
                </TableRowCell>
              );
            }

            return null;
          })
        }
      </TableRow>
    );
  }
}

SeriesEditorRow.propTypes = {
  id: PropTypes.number.isRequired,
  status: PropTypes.string.isRequired,
  titleSlug: PropTypes.string.isRequired,
  title: PropTypes.string.isRequired,
  monitored: PropTypes.bool.isRequired,
  languageProfile: PropTypes.object.isRequired,
  qualityProfile: PropTypes.object.isRequired,
  seriesType: PropTypes.string.isRequired,
  seasonFolder: PropTypes.bool.isRequired,
  path: PropTypes.string.isRequired,
  statistics: PropTypes.object.isRequired,
  tags: PropTypes.arrayOf(PropTypes.number).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  isSelected: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired
};

SeriesEditorRow.defaultProps = {
  tags: []
};

export default SeriesEditorRow;
