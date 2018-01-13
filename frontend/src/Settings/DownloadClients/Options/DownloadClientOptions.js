import PropTypes from 'prop-types';
import React from 'react';
import { inputTypes, sizes } from 'Helpers/Props';
import FieldSet from 'Components/FieldSet';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputGroup from 'Components/Form/FormInputGroup';

function DownloadClientOptions(props) {
  const {
    advancedSettings,
    isFetching,
    error,
    settings,
    hasSettings,
    onInputChange
  } = props;

  return (
    <div>
      {
        isFetching &&
          <LoadingIndicator />
      }

      {
        !isFetching && error &&
          <div>Unable to load download client options</div>
      }

      {
        hasSettings && !isFetching && !error &&
          <div>
            <FieldSet legend="Completed Download Handling">
              <Form>
                <FormGroup size={sizes.MEDIUM}>
                  <FormLabel>Enable</FormLabel>

                  <FormInputGroup
                    type={inputTypes.CHECK}
                    name="enableCompletedDownloadHandling"
                    helpText="Automatically import completed downloads from download client"
                    onChange={onInputChange}
                    {...settings.enableCompletedDownloadHandling}
                  />
                </FormGroup>

                <FormGroup
                  advancedSettings={advancedSettings}
                  isAdvanced={true}
                  size={sizes.MEDIUM}
                >
                  <FormLabel>Remove</FormLabel>

                  <FormInputGroup
                    type={inputTypes.CHECK}
                    name="removeCompletedDownloads"
                    helpText="Remove imported downloads from download client history"
                    onChange={onInputChange}
                    {...settings.removeCompletedDownloads}
                  />
                </FormGroup>
              </Form>
            </FieldSet>

            <FieldSet
              legend="Failed Download Handling"
            >
              <Form>
                <FormGroup size={sizes.MEDIUM}>
                  <FormLabel>Redownload</FormLabel>

                  <FormInputGroup
                    type={inputTypes.CHECK}
                    name="autoRedownloadFailed"
                    helpText="Automatically search for and attempt to download a different release"
                    onChange={onInputChange}
                    {...settings.autoRedownloadFailed}
                  />
                </FormGroup>

                <FormGroup
                  advancedSettings={advancedSettings}
                  isAdvanced={true}
                  size={sizes.MEDIUM}
                >
                  <FormLabel>Remove</FormLabel>

                  <FormInputGroup
                    type={inputTypes.CHECK}
                    name="removeFailedDownloads"
                    helpText="Remove failed downloads from download client history"
                    onChange={onInputChange}
                    {...settings.removeFailedDownloads}
                  />
                </FormGroup>
              </Form>
            </FieldSet>
          </div>
      }
    </div>
  );
}

DownloadClientOptions.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  settings: PropTypes.object.isRequired,
  hasSettings: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default DownloadClientOptions;
