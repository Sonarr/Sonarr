import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, sizes } from 'Helpers/Props';
import styles from './ImportCustomFormatModalContent.css';

class ImportCustomFormatModalContent extends Component {

  //
  // Lifecycle
  constructor(props, context) {
    super(props, context);

    this._importTimeout = null;

    this.state = {
      json: '',
      isSpinning: false,
      parseError: null
    };
  }

  componentWillUnmount() {
    if (this._importTimeout) {
      clearTimeout(this._importTimeout);
    }
  }

  //
  // Control

  onChange = (event) => {
    this.setState({ json: event.value });
  };

  onImportPress = () => {
    this.setState({ isSpinning: true });
    // this is a bodge as we need to register a isSpinning: true to get the spinner button to update
    this._importTimeout = setTimeout(this.doImport, 250);
  };

  doImport = () => {
    const parseError = this.props.onImportPress(this.state.json);
    this.setState({
      parseError,
      isSpinning: false
    });

    if (!parseError) {
      this.props.onModalClose();
    }
  };

  //
  // Render

  render() {
    const {
      isFetching,
      error,
      specificationsPopulated,
      onModalClose
    } = this.props;

    const {
      json,
      isSpinning,
      parseError
    } = this.state;

    return (
      <ModalContent onModalClose={onModalClose}>

        <ModalHeader>
          Import Custom Format
        </ModalHeader>

        <ModalBody>
          <div>
            {
              isFetching &&
                <LoadingIndicator />
            }

            {
              !isFetching && !!error &&
                <div>
                  Unable to load custom formats
                </div>
            }

            {
              !isFetching && !error && specificationsPopulated &&
                <Form>
                  <FormGroup size={sizes.MEDIUM}>
                    <FormLabel>
                      Custom Format JSON
                    </FormLabel>
                    <FormInputGroup
                      key={0}
                      inputClassName={styles.input}
                      type={inputTypes.TEXT_AREA}
                      name="customFormatJson"
                      value={json}
                      onChange={this.onChange}
                      placeholder={'{\n  "name": "Custom Format"\n}'}
                      errors={parseError ? [parseError] : []}
                    />
                  </FormGroup>
                </Form>
            }
          </div>
        </ModalBody>
        <ModalFooter>
          <Button
            onPress={onModalClose}
          >
            Cancel
          </Button>
          <SpinnerErrorButton
            onPress={this.onImportPress}
            isSpinning={isSpinning}
            error={parseError}
          >
            Import
          </SpinnerErrorButton>
        </ModalFooter>
      </ModalContent>
    );
  }
}

ImportCustomFormatModalContent.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  specificationsPopulated: PropTypes.bool.isRequired,
  onImportPress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default ImportCustomFormatModalContent;
