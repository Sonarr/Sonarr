import _ from 'lodash';

function getValidationFailures(saveError) {
  if (!saveError || saveError.status !== 400) {
    return [];
  }

  return _.cloneDeep(saveError.responseJSON);
}

function mapFailure(failure) {
  return {
    message: failure.errorMessage,
    link: failure.infoLink,
    detailedMessage: failure.detailedDescription
  };
}

function selectSettings(item, pendingChanges, saveError) {
  const validationFailures = getValidationFailures(saveError);

  // Merge all settings from the item along with pending
  // changes to ensure any settings that were not included
  // with the item are included.
  const allSettings = Object.assign({}, item, pendingChanges);

  const settings = _.reduce(allSettings, (result, value, key) => {
    if (key === 'fields') {
      return result;
    }

    // Return a flattened value
    if (key === 'implementationName') {
      result.implementationName = item[key];

      return result;
    }

    const setting = {
      value: item[key],
      errors: _.map(_.remove(validationFailures, (failure) => {
        return failure.propertyName.toLowerCase() === key.toLowerCase() && !failure.isWarning;
      }), mapFailure),

      warnings: _.map(_.remove(validationFailures, (failure) => {
        return failure.propertyName.toLowerCase() === key.toLowerCase() && failure.isWarning;
      }), mapFailure)
    };

    if (pendingChanges.hasOwnProperty(key)) {
      setting.previousValue = setting.value;
      setting.value = pendingChanges[key];
      setting.pending = true;
    }

    result[key] = setting;
    return result;
  }, {});

  const fields = _.reduce(item.fields, (result, f) => {
    const field = Object.assign({ pending: false }, f);
    const hasPendingFieldChange = pendingChanges.fields && pendingChanges.fields.hasOwnProperty(field.name);

    if (hasPendingFieldChange) {
      field.previousValue = field.value;
      field.value = pendingChanges.fields[field.name];
      field.pending = true;
    }

    field.errors = _.map(_.remove(validationFailures, (failure) => {
      return failure.propertyName.toLowerCase() === field.name.toLowerCase() && !failure.isWarning;
    }), mapFailure);

    field.warnings = _.map(_.remove(validationFailures, (failure) => {
      return failure.propertyName.toLowerCase() === field.name.toLowerCase() && failure.isWarning;
    }), mapFailure);

    result.push(field);
    return result;
  }, []);

  if (fields.length) {
    settings.fields = fields;
  }

  const validationErrors = _.filter(validationFailures, (failure) => {
    return !failure.isWarning;
  });

  const validationWarnings = _.filter(validationFailures, (failure) => {
    return failure.isWarning;
  });

  return {
    settings,
    validationErrors,
    validationWarnings,
    hasPendingChanges: !_.isEmpty(pendingChanges),
    hasSettings: !_.isEmpty(settings),
    pendingChanges
  };
}

export default selectSettings;
