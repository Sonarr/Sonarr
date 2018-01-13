import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons } from 'Helpers/Props';
import IconButton from 'Components/Link/IconButton';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import EditSeriesModalConnector from 'Series/Edit/EditSeriesModalConnector';
import DeleteSeriesModal from 'Series/Delete/DeleteSeriesModal';

class SeriesIndexActionsCell extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isEditSeriesModalOpen: false,
      isDeleteSeriesModalOpen: false
    };
  }

  //
  // Listeners

  onEditSeriesPress = () => {
    this.setState({ isEditSeriesModalOpen: true });
  }

  onEditSeriesModalClose = () => {
    this.setState({ isEditSeriesModalOpen: false });
  }

  onDeleteSeriesPress = () => {
    this.setState({
      isEditSeriesModalOpen: false,
      isDeleteSeriesModalOpen: true
    });
  }

  onDeleteSeriesModalClose = () => {
    this.setState({ isDeleteSeriesModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      id,
      isRefreshingSeries,
      onRefreshSeriesPress,
      ...otherProps
    } = this.props;

    const {
      isEditSeriesModalOpen,
      isDeleteSeriesModalOpen
    } = this.state;

    return (
      <VirtualTableRowCell
        {...otherProps}
      >
        <SpinnerIconButton
          name={icons.REFRESH}
          title="Refresh series"
          isSpinning={isRefreshingSeries}
          onPress={onRefreshSeriesPress}
        />

        <IconButton
          name={icons.EDIT}
          title="Edit Series"
          onPress={this.onEditSeriesPress}
        />

        <EditSeriesModalConnector
          isOpen={isEditSeriesModalOpen}
          seriesId={id}
          onModalClose={this.onEditSeriesModalClose}
          onDeleteSeriesPress={this.onDeleteSeriesPress}
        />

        <DeleteSeriesModal
          isOpen={isDeleteSeriesModalOpen}
          seriesId={id}
          onModalClose={this.onDeleteSeriesModalClose}
        />
      </VirtualTableRowCell>
    );
  }
}

SeriesIndexActionsCell.propTypes = {
  id: PropTypes.number.isRequired,
  isRefreshingSeries: PropTypes.bool.isRequired,
  onRefreshSeriesPress: PropTypes.func.isRequired
};

export default SeriesIndexActionsCell;
