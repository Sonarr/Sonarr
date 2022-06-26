import PropTypes from 'prop-types';
import React from 'react';
import FieldSet from 'Components/FieldSet';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import { inputTypes, sizes } from 'Helpers/Props';

function AnalyticSettings(props) {
  const {
    settings,
    onInputChange
  } = props;

  const {
    analyticsEnabled
  } = settings;

  return (
    <FieldSet legend="Analytics">
      <FormGroup size={sizes.MEDIUM}>
        <FormLabel>Send Anonymous Usage Data</FormLabel>

        <FormInputGroup
          type={inputTypes.CHECK}
          name="analyticsEnabled"
          helpText="Send anonymous usage and error information to Sonarr's servers. This includes information on your browser, which Sonarr WebUI pages you use, error reporting as well as OS and runtime version. We will use this information to prioritize features and bug fixes."
          helpTextWarning="Requires restart to take effect"
          onChange={onInputChange}
          {...analyticsEnabled}
        />
      </FormGroup>
    </FieldSet>
  );
}

AnalyticSettings.propTypes = {
  settings: PropTypes.object.isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default AnalyticSettings;
