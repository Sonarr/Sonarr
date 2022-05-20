import PropTypes from 'prop-types';
import React from 'react';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import { inputTypes, kinds, sizes } from 'Helpers/Props';

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
        hasSettings && !isFetching && !error && advancedSettings &&
          <div>
            <FieldSet legend="Completed Download Handling">

              <Form>
                <FormGroup
                  advancedSettings={advancedSettings}
                  isAdvanced={true}
                  size={sizes.MEDIUM}
                >
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
                  <FormLabel>Redownload Failed</FormLabel>

                  <FormInputGroup
                    type={inputTypes.CHECK}
                    name="autoRedownloadFailed"
                    helpText="Automatically search for and attempt to download a different release"
                    onChange={onInputChange}
                    {...settings.autoRedownloadFailed}
                  />
                </FormGroup>
              </Form>

              <Alert kind={kinds.INFO}>
                The Remove settings were moved to the individual Download Client settings in the table above.
              </Alert>
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
