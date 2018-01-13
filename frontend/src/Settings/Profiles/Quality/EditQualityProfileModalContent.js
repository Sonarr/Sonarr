import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Measure from 'react-measure';
import { inputTypes, kinds, sizes } from 'Helpers/Props';
import dimensions from 'Styles/Variables/dimensions';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputGroup from 'Components/Form/FormInputGroup';
import QualityProfileItems from './QualityProfileItems';
import styles from './EditQualityProfileModalContent.css';

const MODAL_BODY_PADDING = parseInt(dimensions.modalBodyPadding);

class EditQualityProfileModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      headerHeight: 0,
      bodyHeight: 0,
      footerHeight: 0
    };
  }

  componentDidUpdate(prevProps, prevState) {
    const {
      headerHeight,
      bodyHeight,
      footerHeight
    } = this.state;

    if (
      headerHeight > 0 &&
      bodyHeight > 0 &&
      footerHeight > 0 &&
      (
        headerHeight !== prevState.headerHeight ||
        bodyHeight !== prevState.bodyHeight ||
        footerHeight !== prevState.footerHeight
      )
    ) {
      const padding = MODAL_BODY_PADDING * 2;

      this.props.onContentHeightChange(
        headerHeight + bodyHeight + footerHeight + padding
      );
    }
  }

  //
  // Listeners

  onHeaderMeasure = ({ height }) => {
    if (height > this.state.headerHeight) {
      this.setState({ headerHeight: height });
    }
  }

  onBodyMeasure = ({ height }) => {

    if (height > this.state.bodyHeight) {
      this.setState({ bodyHeight: height });
    }
  }

  onFooterMeasure = ({ height }) => {
    if (height > this.state.footerHeight) {
      this.setState({ footerHeight: height });
    }
  }

  //
  // Render

  render() {
    const {
      editGroups,
      isFetching,
      error,
      isSaving,
      saveError,
      qualities,
      item,
      isInUse,
      onInputChange,
      onCutoffChange,
      onSavePress,
      onModalClose,
      onDeleteQualityProfilePress,
      ...otherProps
    } = this.props;

    const {
      id,
      name,
      cutoff,
      items
    } = item;

    return (
      <ModalContent onModalClose={onModalClose}>
        <Measure
          whitelist={['height']}
          includeMargin={false}
          onMeasure={this.onHeaderMeasure}
        >
          <ModalHeader>
            {id ? 'Edit Quality Profile' : 'Add Quality Profile'}
          </ModalHeader>
        </Measure>

        <ModalBody>
          <Measure
            whitelist={['height']}
            onMeasure={this.onBodyMeasure}
          >
            <div>
              {
                isFetching &&
                  <LoadingIndicator />
              }

              {
                !isFetching && !!error &&
                  <div>Unable to add a new quality profile, please try again.</div>
              }

              {
                !isFetching && !error &&
                  <Form
                    {...otherProps}
                  >
                    <div className={styles.formGroupsContainer}>
                      <div className={styles.formGroupWrapper}>
                        <FormGroup size={sizes.EXTRA_SMALL}>
                          <FormLabel size={sizes.small}>
                            Name
                          </FormLabel>

                          <FormInputGroup
                            type={inputTypes.TEXT}
                            name="name"
                            {...name}
                            onChange={onInputChange}
                          />
                        </FormGroup>

                        <FormGroup size={sizes.EXTRA_SMALL}>
                          <FormLabel size={sizes.small}>
                            Cutoff
                          </FormLabel>

                          <FormInputGroup
                            type={inputTypes.SELECT}
                            name="cutoff"
                            {...cutoff}
                            values={qualities}
                            helpText="Once this quality is reached Sonarr will no longer download episodes"
                            onChange={onCutoffChange}
                          />
                        </FormGroup>
                      </div>

                      <div className={styles.formGroupWrapper}>
                        <QualityProfileItems
                          editGroups={editGroups}
                          qualityProfileItems={items.value}
                          errors={items.errors}
                          warnings={items.warnings}
                          {...otherProps}
                        />
                      </div>
                    </div>
                  </Form>

              }
            </div>
          </Measure>
        </ModalBody>

        <Measure
          whitelist={['height']}
          includeMargin={false}
          onMeasure={this.onFooterMeasure}
        >
          <ModalFooter>
            {
              id &&
                <div
                  className={styles.deleteButtonContainer}
                  title={isInUse && 'Can\'t delete a quality profile that is attached to a series'}
                >
                  <Button
                    kind={kinds.DANGER}
                    isDisabled={isInUse}
                    onPress={onDeleteQualityProfilePress}
                  >
                    Delete
                  </Button>
                </div>
            }

            <Button
              onPress={onModalClose}
            >
              Cancel
            </Button>

            <SpinnerErrorButton
              isSpinning={isSaving}
              error={saveError}
              onPress={onSavePress}
            >
              Save
            </SpinnerErrorButton>
          </ModalFooter>
        </Measure>
      </ModalContent>
    );
  }
}

EditQualityProfileModalContent.propTypes = {
  editGroups: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  qualities: PropTypes.arrayOf(PropTypes.object).isRequired,
  item: PropTypes.object.isRequired,
  isInUse: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onCutoffChange: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onContentHeightChange: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onDeleteQualityProfilePress: PropTypes.func
};

export default EditQualityProfileModalContent;
