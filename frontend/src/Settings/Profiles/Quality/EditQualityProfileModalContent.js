import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Measure from 'Components/Measure';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds, sizes } from 'Helpers/Props';
import dimensions from 'Styles/Variables/dimensions';
import translate from 'Utilities/String/translate';
import QualityProfileFormatItems from './QualityProfileFormatItems';
import QualityProfileItems from './QualityProfileItems';
import styles from './EditQualityProfileModalContent.css';

const MODAL_BODY_PADDING = parseInt(dimensions.modalBodyPadding);

function getCustomFormatRender(formatItems, otherProps) {
  return (
    <QualityProfileFormatItems
      profileFormatItems={formatItems.value}
      errors={formatItems.errors}
      warnings={formatItems.warnings}
      {...otherProps}
    />
  );
}

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
  };

  onBodyMeasure = ({ height }) => {

    if (height > this.state.bodyHeight) {
      this.setState({ bodyHeight: height });
    }
  };

  onFooterMeasure = ({ height }) => {
    if (height > this.state.footerHeight) {
      this.setState({ footerHeight: height });
    }
  };

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
      customFormats,
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
      upgradeAllowed,
      cutoff,
      minFormatScore,
      cutoffFormatScore,
      items,
      formatItems
    } = item;

    return (
      <ModalContent onModalClose={onModalClose}>
        <Measure
          whitelist={['height']}
          includeMargin={false}
          onMeasure={this.onHeaderMeasure}
        >
          <ModalHeader>
            {id ? translate('EditQualityProfile') : translate('AddQualityProfile')}
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
                  <div>
                    {translate('AddQualityProfileError')}
                  </div>
              }

              {
                !isFetching && !error &&
                  <Form
                    {...otherProps}
                  >
                    <div className={styles.formGroupsContainer}>
                      <div className={styles.formGroupWrapper}>
                        <FormGroup size={sizes.EXTRA_SMALL}>
                          <FormLabel size={sizes.SMALL}>
                            {translate('Name')}
                          </FormLabel>

                          <FormInputGroup
                            type={inputTypes.TEXT}
                            name="name"
                            {...name}
                            onChange={onInputChange}
                          />
                        </FormGroup>

                        <FormGroup size={sizes.EXTRA_SMALL}>
                          <FormLabel size={sizes.SMALL}>
                            {translate('UpgradesAllowed')}
                          </FormLabel>

                          <FormInputGroup
                            type={inputTypes.CHECK}
                            name="upgradeAllowed"
                            {...upgradeAllowed}
                            helpText={translate('UpgradesAllowedHelpText')}
                            onChange={onInputChange}
                          />
                        </FormGroup>

                        {
                          upgradeAllowed.value &&
                            <FormGroup size={sizes.EXTRA_SMALL}>
                              <FormLabel size={sizes.SMALL}>
                                {translate('UpgradeUntil')}
                              </FormLabel>

                              <FormInputGroup
                                type={inputTypes.SELECT}
                                name="cutoff"
                                {...cutoff}
                                values={qualities}
                                helpText={translate('UpgradeUntilEpisodeHelpText')}
                                onChange={onCutoffChange}
                              />
                            </FormGroup>
                        }

                        {
                          formatItems.value.length > 0 &&
                            <FormGroup size={sizes.EXTRA_SMALL}>
                              <FormLabel size={sizes.SMALL}>
                                {translate('MinimumCustomFormatScore')}
                              </FormLabel>

                              <FormInputGroup
                                type={inputTypes.NUMBER}
                                name="minFormatScore"
                                {...minFormatScore}
                                helpText={translate('MinimumCustomFormatScoreHelpText')}
                                onChange={onInputChange}
                              />
                            </FormGroup>
                        }

                        {
                          upgradeAllowed.value && formatItems.value.length > 0 &&
                            <FormGroup size={sizes.EXTRA_SMALL}>
                              <FormLabel size={sizes.SMALL}>
                                {translate('UpgradeUntilCustomFormatScore')}
                              </FormLabel>

                              <FormInputGroup
                                type={inputTypes.NUMBER}
                                name="cutoffFormatScore"
                                {...cutoffFormatScore}
                                helpText={translate('UpgradeUntilCustomFormatScoreEpisodeHelpText')}
                                onChange={onInputChange}
                              />
                            </FormGroup>
                        }

                        <div className={styles.formatItemLarge}>
                          {getCustomFormatRender(formatItems, otherProps)}
                        </div>
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

                      <div className={styles.formatItemSmall}>
                        {getCustomFormatRender(formatItems, otherProps)}
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
              id ?
                <div
                  className={styles.deleteButtonContainer}
                  title={
                    isInUse ?
                      translate('QualityProfileInUseSeriesListCollection') :
                      undefined
                  }
                >
                  <Button
                    kind={kinds.DANGER}
                    isDisabled={isInUse}
                    onPress={onDeleteQualityProfilePress}
                  >
                    {translate('Delete')}
                  </Button>
                </div> :
                null
            }

            <Button
              onPress={onModalClose}
            >
              {translate('Cancel')}
            </Button>

            <SpinnerErrorButton
              isSpinning={isSaving}
              error={saveError}
              onPress={onSavePress}
            >
              {translate('Save')}
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
  customFormats: PropTypes.arrayOf(PropTypes.object).isRequired,
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
