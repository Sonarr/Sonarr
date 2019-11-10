import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { kinds } from 'Helpers/Props';
import Card from 'Components/Card';
import Label from 'Components/Label';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import EditImportListModalConnector from './EditImportListModalConnector';
import styles from './ImportList.css';

class ImportList extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isEditImportListModalOpen: false,
      isDeleteImportListModalOpen: false
    };
  }

  //
  // Listeners

  onEditImportListPress = () => {
    this.setState({ isEditImportListModalOpen: true });
  }

  onEditImportListModalClose = () => {
    this.setState({ isEditImportListModalOpen: false });
  }

  onDeleteImportListPress = () => {
    this.setState({
      isEditImportListModalOpen: false,
      isDeleteImportListModalOpen: true
    });
  }

  onDeleteImportListModalClose= () => {
    this.setState({ isDeleteImportListModalOpen: false });
  }

  onConfirmDeleteImportList = () => {
    this.props.onConfirmDeleteImportList(this.props.id);
  }

  //
  // Render

  render() {
    const {
      id,
      name,
      enableAutomaticAdd
    } = this.props;

    return (
      <Card
        className={styles.list}
        overlayContent={true}
        onPress={this.onEditImportListPress}
      >
        <div className={styles.name}>
          {name}
        </div>

        <div className={styles.enabled}>
          {
            enableAutomaticAdd &&
              <Label kind={kinds.SUCCESS}>
                Automatic Add
              </Label>
          }

        </div>

        <EditImportListModalConnector
          id={id}
          isOpen={this.state.isEditImportListModalOpen}
          onModalClose={this.onEditImportListModalClose}
          onDeleteImportListPress={this.onDeleteImportListPress}
        />

        <ConfirmModal
          isOpen={this.state.isDeleteImportListModalOpen}
          kind={kinds.DANGER}
          title="Delete Import List"
          message={`Are you sure you want to delete the list '${name}'?`}
          confirmLabel="Delete"
          onConfirm={this.onConfirmDeleteImportList}
          onCancel={this.onDeleteImportListModalClose}
        />
      </Card>
    );
  }
}

ImportList.propTypes = {
  id: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  enableAutomaticAdd: PropTypes.bool.isRequired,
  onConfirmDeleteImportList: PropTypes.func.isRequired
};

export default ImportList;
