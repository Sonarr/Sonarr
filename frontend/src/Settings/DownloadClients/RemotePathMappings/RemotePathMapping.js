import classNames from 'classnames';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import { icons, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import EditRemotePathMappingModalConnector from './EditRemotePathMappingModalConnector';
import styles from './RemotePathMapping.css';

class RemotePathMapping extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isEditRemotePathMappingModalOpen: false,
      isDeleteRemotePathMappingModalOpen: false
    };
  }

  //
  // Listeners

  onEditRemotePathMappingPress = () => {
    this.setState({ isEditRemotePathMappingModalOpen: true });
  };

  onEditRemotePathMappingModalClose = () => {
    this.setState({ isEditRemotePathMappingModalOpen: false });
  };

  onDeleteRemotePathMappingPress = () => {
    this.setState({
      isEditRemotePathMappingModalOpen: false,
      isDeleteRemotePathMappingModalOpen: true
    });
  };

  onDeleteRemotePathMappingModalClose = () => {
    this.setState({ isDeleteRemotePathMappingModalOpen: false });
  };

  onConfirmDeleteRemotePathMapping = () => {
    this.props.onConfirmDeleteRemotePathMapping(this.props.id);
  };

  //
  // Render

  render() {
    const {
      id,
      host,
      remotePath,
      localPath
    } = this.props;

    return (
      <div
        className={classNames(
          styles.remotePathMapping
        )}
      >
        <div className={styles.host}>{host}</div>
        <div className={styles.path}>{remotePath}</div>
        <div className={styles.path}>{localPath}</div>

        <div className={styles.actions}>
          <Link
            onPress={this.onEditRemotePathMappingPress}
          >
            <Icon name={icons.EDIT} />
          </Link>
        </div>

        <EditRemotePathMappingModalConnector
          id={id}
          isOpen={this.state.isEditRemotePathMappingModalOpen}
          onModalClose={this.onEditRemotePathMappingModalClose}
          onDeleteRemotePathMappingPress={this.onDeleteRemotePathMappingPress}
        />

        <ConfirmModal
          isOpen={this.state.isDeleteRemotePathMappingModalOpen}
          kind={kinds.DANGER}
          title={translate('DeleteRemotePathMapping')}
          message={translate('DeleteRemotePathMappingMessageText')}
          confirmLabel={translate('Delete')}
          onConfirm={this.onConfirmDeleteRemotePathMapping}
          onCancel={this.onDeleteRemotePathMappingModalClose}
        />
      </div>
    );
  }
}

RemotePathMapping.propTypes = {
  id: PropTypes.number.isRequired,
  host: PropTypes.string.isRequired,
  remotePath: PropTypes.string.isRequired,
  localPath: PropTypes.string.isRequired,
  onConfirmDeleteRemotePathMapping: PropTypes.func.isRequired
};

RemotePathMapping.defaultProps = {
  // The drag preview will not connect the drag handle.
  connectDragSource: (node) => node
};

export default RemotePathMapping;
