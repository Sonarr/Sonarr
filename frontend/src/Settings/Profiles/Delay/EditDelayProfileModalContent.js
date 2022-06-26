import PropTypes from 'prop-types';
import React from 'react';
import Alert from 'Components/Alert';
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
import { inputTypes, kinds } from 'Helpers/Props';
import { boolSettingShape, numberSettingShape, tagSettingShape } from 'Helpers/Props/Shapes/settingShape';
import styles from './EditDelayProfileModalContent.css';

const protocolOptions = [
  { key: 'preferUsenet', value: 'Prefer Usenet' },
  { key: 'preferTorrent', value: 'Prefer Torrent' },
  { key: 'onlyUsenet', value: 'Only Usenet' },
  { key: 'onlyTorrent', value: 'Only Torrent' }
];

function EditDelayProfileModalContent(props) {
  const {
    id,
    isFetching,
    error,
    isSaving,
    saveError,
    item,
    protocol,
    onInputChange,
    onProtocolChange,
    onSavePress,
    onModalClose,
    onDeleteDelayProfilePress,
    ...otherProps
  } = props;

  const {
    enableUsenet,
    enableTorrent,
    usenetDelay,
    torrentDelay,
    bypassIfHighestQuality,
    tags
  } = item;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id ? 'Edit Delay Profile' : 'Add Delay Profile'}
      </ModalHeader>

      <ModalBody>
        {
          isFetching ?
            <LoadingIndicator /> :
            null
        }

        {
          !isFetching && !!error ?
            <div>Unable to add a new quality profile, please try again.</div> :
            null
        }

        {
          !isFetching && !error ?
            <Form {...otherProps}>
              <FormGroup>
                <FormLabel>Preferred Protocol</FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="protocol"
                  value={protocol}
                  values={protocolOptions}
                  helpText="Choose which protocol(s) to use and which one is preferred when choosing between otherwise equal releases"
                  onChange={onProtocolChange}
                />
              </FormGroup>

              {
                enableUsenet.value &&
                  <FormGroup>
                    <FormLabel>Usenet Delay</FormLabel>

                    <FormInputGroup
                      type={inputTypes.NUMBER}
                      name="usenetDelay"
                      unit="minutes"
                      {...usenetDelay}
                      helpText="Delay in minutes to wait before grabbing a release from Usenet"
                      onChange={onInputChange}
                    />
                  </FormGroup>
              }

              {
                enableTorrent.value &&
                  <FormGroup>
                    <FormLabel>Torrent Delay</FormLabel>

                    <FormInputGroup
                      type={inputTypes.NUMBER}
                      name="torrentDelay"
                      unit="minutes"
                      {...torrentDelay}
                      helpText="Delay in minutes to wait before grabbing a torrent"
                      onChange={onInputChange}
                    />
                  </FormGroup>
              }

              {
                <FormGroup>
                  <FormLabel>Bypass if Highest Quality</FormLabel>

                  <FormInputGroup
                    type={inputTypes.CHECK}
                    name="bypassIfHighestQuality"
                    {...bypassIfHighestQuality}
                    helpText="Bypass delay when release has the highest enabled quality in the quality profile with the preferred protocol"
                    onChange={onInputChange}
                  />
                </FormGroup>
              }

              {
                id === 1 ?
                  <Alert>
                    This is the default profile. It applies to all series that don't have an explicit profile.
                  </Alert> :

                  <FormGroup>
                    <FormLabel>Tags</FormLabel>

                    <FormInputGroup
                      type={inputTypes.TAG}
                      name="tags"
                      {...tags}
                      helpText="Applies to series with at least one matching tag"
                      onChange={onInputChange}
                    />
                  </FormGroup>
              }
            </Form> :
            null
        }
      </ModalBody>
      <ModalFooter>
        {
          id && id > 1 ?
            <Button
              className={styles.deleteButton}
              kind={kinds.DANGER}
              onPress={onDeleteDelayProfilePress}
            >
              Delete
            </Button> :
            null
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
    </ModalContent>
  );
}

const delayProfileShape = {
  enableUsenet: PropTypes.shape(boolSettingShape).isRequired,
  enableTorrent: PropTypes.shape(boolSettingShape).isRequired,
  usenetDelay: PropTypes.shape(numberSettingShape).isRequired,
  torrentDelay: PropTypes.shape(numberSettingShape).isRequired,
  order: PropTypes.shape(numberSettingShape),
  tags: PropTypes.shape(tagSettingShape).isRequired
};

EditDelayProfileModalContent.propTypes = {
  id: PropTypes.number,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.shape(delayProfileShape).isRequired,
  protocol: PropTypes.string.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onProtocolChange: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onDeleteDelayProfilePress: PropTypes.func
};

export default EditDelayProfileModalContent;
