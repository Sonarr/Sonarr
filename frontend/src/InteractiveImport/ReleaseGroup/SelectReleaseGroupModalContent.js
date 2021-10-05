import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { inputTypes, kinds } from 'Helpers/Props';
import Button from 'Components/Link/Button';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputGroup from 'Components/Form/FormInputGroup';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';

class SelectReleaseGroupModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    const {
      releaseGroup
    } = props;

    this.state = {
      releaseGroup
    };
  }

  //
  // Listeners

  onReleaseGroupChange = ({ value }) => {
    this.setState({ releaseGroup: value });
  }

  onReleaseGroupSelect = () => {
    this.props.onReleaseGroupSelect(this.state);
  }

  //
  // Render

  render() {
    const {
      onModalClose
    } = this.props;

    const {
      releaseGroup
    } = this.state;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Manual Import - Set Release Group
        </ModalHeader>

        <ModalBody>
          <Form>
            <FormGroup>
              <FormLabel>Release Group</FormLabel>

              <FormInputGroup
                type={inputTypes.TEXT}
                name="releaseGroup"
                value={releaseGroup}
                onChange={this.onReleaseGroupChange}
              />
            </FormGroup>
          </Form>
        </ModalBody>

        <ModalFooter>
          <Button onPress={onModalClose}>
            Cancel
          </Button>

          <Button
            kind={kinds.SUCCESS}
            onPress={this.onReleaseGroupSelect}
          >
            Set Release Group
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

SelectReleaseGroupModalContent.propTypes = {
  releaseGroup: PropTypes.string.isRequired,
  onReleaseGroupSelect: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default SelectReleaseGroupModalContent;
