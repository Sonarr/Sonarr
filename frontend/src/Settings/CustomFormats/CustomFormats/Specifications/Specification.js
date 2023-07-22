import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Card from 'Components/Card';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import { icons, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import EditSpecificationModalConnector from './EditSpecificationModal';
import styles from './Specification.css';

class Specification extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isEditSpecificationModalOpen: false,
      isDeleteSpecificationModalOpen: false
    };
  }

  //
  // Listeners

  onEditSpecificationPress = () => {
    this.setState({ isEditSpecificationModalOpen: true });
  };

  onEditSpecificationModalClose = () => {
    this.setState({ isEditSpecificationModalOpen: false });
  };

  onDeleteSpecificationPress = () => {
    this.setState({
      isEditSpecificationModalOpen: false,
      isDeleteSpecificationModalOpen: true
    });
  };

  onDeleteSpecificationModalClose = () => {
    this.setState({ isDeleteSpecificationModalOpen: false });
  };

  onCloneSpecificationPress = () => {
    this.props.onCloneSpecificationPress(this.props.id);
  };

  onConfirmDeleteSpecification = () => {
    this.props.onConfirmDeleteSpecification(this.props.id);
  };

  //
  // Lifecycle

  render() {
    const {
      id,
      implementationName,
      name,
      required,
      negate
    } = this.props;

    return (
      <Card
        className={styles.customFormat}
        overlayContent={true}
        onPress={this.onEditSpecificationPress}
      >
        <div className={styles.nameContainer}>
          <div className={styles.name}>
            {name}
          </div>

          <IconButton
            className={styles.cloneButton}
            title={translate('CloneCondition')}
            name={icons.CLONE}
            onPress={this.onCloneSpecificationPress}
          />
        </div>

        <div className={styles.labels}>
          <Label kind={kinds.DEFAULT}>
            {implementationName}
          </Label>

          {
            negate &&
              <Label kind={kinds.DANGER}>
                {translate('Negated')}
              </Label>
          }

          {
            required &&
              <Label kind={kinds.SUCCESS}>
                {translate('Required')}
              </Label>
          }
        </div>

        <EditSpecificationModalConnector
          id={id}
          isOpen={this.state.isEditSpecificationModalOpen}
          onModalClose={this.onEditSpecificationModalClose}
          onDeleteSpecificationPress={this.onDeleteSpecificationPress}
        />

        <ConfirmModal
          isOpen={this.state.isDeleteSpecificationModalOpen}
          kind={kinds.DANGER}
          title={translate('DeleteCondition')}
          message={translate('DeleteConditionMessageText', { name })}
          confirmLabel={translate('Delete')}
          onConfirm={this.onConfirmDeleteSpecification}
          onCancel={this.onDeleteSpecificationModalClose}
        />
      </Card>
    );
  }
}

Specification.propTypes = {
  id: PropTypes.number.isRequired,
  implementation: PropTypes.string.isRequired,
  implementationName: PropTypes.string.isRequired,
  name: PropTypes.string.isRequired,
  negate: PropTypes.bool.isRequired,
  required: PropTypes.bool.isRequired,
  fields: PropTypes.arrayOf(PropTypes.object).isRequired,
  onConfirmDeleteSpecification: PropTypes.func.isRequired,
  onCloneSpecificationPress: PropTypes.func.isRequired
};

export default Specification;
