import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import TableRowButton from 'Components/Table/TableRowButton';

class SelectEpisodeRow extends Component {

  //
  // Listeners

  onPress = () => {
    const {
      id,
      isSelected
    } = this.props;

    this.props.onSelectedChange({ id, value: !isSelected });
  };

  //
  // Render

  render() {
    const {
      id,
      episodeNumber,
      absoluteEpisodeNumber,
      title,
      airDate,
      isAnime,
      isSelected,
      onSelectedChange
    } = this.props;

    return (
      <TableRowButton onPress={this.onPress}>
        <TableSelectCell
          id={id}
          isSelected={isSelected}
          onSelectedChange={onSelectedChange}
        />

        <TableRowCell>
          {episodeNumber}
          {isAnime ? ` (${absoluteEpisodeNumber})` : ''}
        </TableRowCell>

        <TableRowCell>
          {title}
        </TableRowCell>

        <TableRowCell>
          {airDate}
        </TableRowCell>
      </TableRowButton>
    );
  }
}

SelectEpisodeRow.propTypes = {
  id: PropTypes.number.isRequired,
  episodeNumber: PropTypes.number.isRequired,
  absoluteEpisodeNumber: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  airDate: PropTypes.string.isRequired,
  isAnime: PropTypes.bool.isRequired,
  isSelected: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired
};

export default SelectEpisodeRow;
