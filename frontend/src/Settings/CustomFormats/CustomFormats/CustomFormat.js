import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Card from 'Components/Card';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import { icons, kinds } from 'Helpers/Props';
import EditCustomFormatModalConnector from './EditCustomFormatModalConnector';
import ExportCustomFormatModal from './ExportCustomFormatModal';
import styles from './CustomFormat.css';

class CustomFormat extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isEditCustomFormatModalOpen: false,
      isExportCustomFormatModalOpen: false,
      isDeleteCustomFormatModalOpen: false
    };
  }

  //
  // Listeners

  onEditCustomFormatPress = () => {
    this.setState({ isEditCustomFormatModalOpen: true });
  };

  onEditCustomFormatModalClose = () => {
    this.setState({ isEditCustomFormatModalOpen: false });
  };

  onExportCustomFormatPress = () => {
    this.setState({ isExportCustomFormatModalOpen: true });
  };

  onExportCustomFormatModalClose = () => {
    this.setState({ isExportCustomFormatModalOpen: false });
  };

  onDeleteCustomFormatPress = () => {
    this.setState({
      isEditCustomFormatModalOpen: false,
      isDeleteCustomFormatModalOpen: true
    });
  };

  onDeleteCustomFormatModalClose = () => {
    this.setState({ isDeleteCustomFormatModalOpen: false });
  };

  onConfirmDeleteCustomFormat = () => {
    this.props.onConfirmDeleteCustomFormat(this.props.id);
  };

  onCloneCustomFormatPress = () => {
    const {
      id,
      onCloneCustomFormatPress
    } = this.props;

    onCloneCustomFormatPress(id);
  };

  //
  // Render

  render() {
    const {
      id,
      name,
      specifications,
      isDeleting
    } = this.props;

    return (
      <Card
        className={styles.customFormat}
        overlayContent={true}
        onPress={this.onEditCustomFormatPress}
      >
        <div className={styles.nameContainer}>
          <div className={styles.name}>
            {name}
          </div>

          <div>
            <IconButton
              className={styles.cloneButton}
              title="Clone Custom Format"
              name={icons.CLONE}
              onPress={this.onCloneCustomFormatPress}
            />

            <IconButton
              className={styles.cloneButton}
              title="Export Custom Format"
              name={icons.EXPORT}
              onPress={this.onExportCustomFormatPress}
            />
          </div>
        </div>

        <div>
          {
            specifications.map((item, index) => {
              if (!item) {
                return null;
              }

              let kind = kinds.DEFAULT;
              if (item.required) {
                kind = kinds.SUCCESS;
              }
              if (item.negate) {
                kind = kinds.DANGER;
              }

              return (
                <Label
                  key={index}
                  kind={kind}
                >
                  {item.name}
                </Label>
              );
            })
          }
        </div>

        <EditCustomFormatModalConnector
          id={id}
          isOpen={this.state.isEditCustomFormatModalOpen}
          onModalClose={this.onEditCustomFormatModalClose}
          onDeleteCustomFormatPress={this.onDeleteCustomFormatPress}
        />

        <ExportCustomFormatModal
          id={id}
          isOpen={this.state.isExportCustomFormatModalOpen}
          onModalClose={this.onExportCustomFormatModalClose}
        />

        <ConfirmModal
          isOpen={this.state.isDeleteCustomFormatModalOpen}
          kind={kinds.DANGER}
          title="Delete Custom Format"
          message={`Are you sure you want to delete the custom format '${name}'?`}
          confirmLabel="Delete"
          isSpinning={isDeleting}
          onConfirm={this.onConfirmDeleteCustomFormat}
          onCancel={this.onDeleteCustomFormatModalClose}
        />
      </Card>
    );
  }
}

CustomFormat.propTypes = {
  id: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  specifications: PropTypes.arrayOf(PropTypes.object).isRequired,
  isDeleting: PropTypes.bool.isRequired,
  onConfirmDeleteCustomFormat: PropTypes.func.isRequired,
  onCloneCustomFormatPress: PropTypes.func.isRequired
};

export default CustomFormat;
