import PropTypes from 'prop-types';
import React, { Component } from 'react';
import IconButton from 'Components/Link/IconButton';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import TableRow from 'Components/Table/TableRow';
import EpisodeFormats from 'Episode/EpisodeFormats';
import EpisodeLanguage from 'Episode/EpisodeLanguage';
import EpisodeQuality from 'Episode/EpisodeQuality';
import { icons, kinds } from 'Helpers/Props';
import SeriesTitleLink from 'Series/SeriesTitleLink';
import BlocklistDetailsModal from './BlocklistDetailsModal';
import styles from './BlocklistRow.css';

class BlocklistRow extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isDetailsModalOpen: false
    };
  }

  //
  // Listeners

  onDetailsPress = () => {
    this.setState({ isDetailsModalOpen: true });
  };

  onDetailsModalClose = () => {
    this.setState({ isDetailsModalOpen: false });
  };

  //
  // Render

  render() {
    const {
      id,
      series,
      sourceTitle,
      language,
      quality,
      customFormats,
      date,
      protocol,
      indexer,
      message,
      isSelected,
      columns,
      onSelectedChange,
      onRemovePress
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

            if (name === 'series.sortTitle') {
              return (
                <TableRowCell key={name}>
                  <SeriesTitleLink
                    titleSlug={series.titleSlug}
                    title={series.title}
                  />
                </TableRowCell>
              );
            }

            if (name === 'sourceTitle') {
              return (
                <TableRowCell key={name}>
                  {sourceTitle}
                </TableRowCell>
              );
            }

            if (name === 'language') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.language}
                >
                  <EpisodeLanguage
                    language={language}
                  />
                </TableRowCell>
              );
            }

            if (name === 'quality') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.quality}
                >
                  <EpisodeQuality
                    quality={quality}
                  />
                </TableRowCell>
              );
            }

            if (name === 'customFormats') {
              return (
                <TableRowCell key={name}>
                  <EpisodeFormats
                    formats={customFormats}
                  />
                </TableRowCell>
              );
            }

            if (name === 'date') {
              return (
                <RelativeDateCellConnector
                  key={name}
                  date={date}
                />
              );
            }

            if (name === 'indexer') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.indexer}
                >
                  {indexer}
                </TableRowCell>
              );
            }

            if (name === 'actions') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.actions}
                >
                  <IconButton
                    name={icons.INFO}
                    onPress={this.onDetailsPress}
                  />

                  <IconButton
                    title="Remove from blocklist"
                    name={icons.REMOVE}
                    kind={kinds.DANGER}
                    onPress={onRemovePress}
                  />
                </TableRowCell>
              );
            }

            return null;
          })
        }

        <BlocklistDetailsModal
          isOpen={this.state.isDetailsModalOpen}
          sourceTitle={sourceTitle}
          protocol={protocol}
          indexer={indexer}
          message={message}
          onModalClose={this.onDetailsModalClose}
        />
      </TableRow>
    );
  }

}

BlocklistRow.propTypes = {
  id: PropTypes.number.isRequired,
  series: PropTypes.object.isRequired,
  sourceTitle: PropTypes.string.isRequired,
  language: PropTypes.object.isRequired,
  quality: PropTypes.object.isRequired,
  customFormats: PropTypes.arrayOf(PropTypes.object).isRequired,
  date: PropTypes.string.isRequired,
  protocol: PropTypes.string.isRequired,
  indexer: PropTypes.string,
  message: PropTypes.string,
  isSelected: PropTypes.bool.isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  onSelectedChange: PropTypes.func.isRequired,
  onRemovePress: PropTypes.func.isRequired
};

export default BlocklistRow;
