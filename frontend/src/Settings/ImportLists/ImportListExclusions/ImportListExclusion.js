import classNames from 'classnames';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import { icons, kinds } from 'Helpers/Props';
import EditImportListExclusionModalConnector from './EditImportListExclusionModalConnector';
import styles from './ImportListExclusion.css';

class ImportListExclusion extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isEditImportListExclusionModalOpen: false,
      isDeleteImportListExclusionModalOpen: false
    };
  }

  //
  // Listeners

  onEditImportListExclusionPress = () => {
    this.setState({ isEditImportListExclusionModalOpen: true });
  };

  onEditImportListExclusionModalClose = () => {
    this.setState({ isEditImportListExclusionModalOpen: false });
  };

  onDeleteImportListExclusionPress = () => {
    this.setState({
      isEditImportListExclusionModalOpen: false,
      isDeleteImportListExclusionModalOpen: true
    });
  };

  onDeleteImportListExclusionModalClose = () => {
    this.setState({ isDeleteImportListExclusionModalOpen: false });
  };

  onConfirmDeleteImportListExclusion = () => {
    this.props.onConfirmDeleteImportListExclusion(this.props.id);
  };

  //
  // Render

  render() {
    const {
      id,
      title,
      tvdbId
    } = this.props;

    return (
      <div
        className={classNames(
          styles.importListExclusion
        )}
      >
        <div className={styles.title}>{title}</div>
        <div className={styles.tvdbId}>{tvdbId}</div>

        <div className={styles.actions}>
          <Link
            onPress={this.onEditImportListExclusionPress}
          >
            <Icon name={icons.EDIT} />
          </Link>
        </div>

        <EditImportListExclusionModalConnector
          id={id}
          isOpen={this.state.isEditImportListExclusionModalOpen}
          onModalClose={this.onEditImportListExclusionModalClose}
          onDeleteImportListExclusionPress={this.onDeleteImportListExclusionPress}
        />

        <ConfirmModal
          isOpen={this.state.isDeleteImportListExclusionModalOpen}
          kind={kinds.DANGER}
          title="Delete Import List Exclusion"
          message="Are you sure you want to delete this import list exclusion?"
          confirmLabel="Delete"
          onConfirm={this.onConfirmDeleteImportListExclusion}
          onCancel={this.onDeleteImportListExclusionModalClose}
        />
      </div>
    );
  }
}

ImportListExclusion.propTypes = {
  id: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  tvdbId: PropTypes.number.isRequired,
  onConfirmDeleteImportListExclusion: PropTypes.func.isRequired
};

ImportListExclusion.defaultProps = {
  // The drag preview will not connect the drag handle.
  connectDragSource: (node) => node
};

export default ImportListExclusion;
