import { cloneDeep, isEmpty } from 'lodash';
import { Error } from 'App/State/AppSectionState';
import Field from 'typings/Field';
import {
  Failure,
  Pending,
  PendingField,
  PendingSection,
  ValidationError,
  ValidationFailure,
  ValidationWarning,
} from 'typings/pending';

interface ValidationFailures {
  errors: ValidationError[];
  warnings: ValidationWarning[];
}

function getValidationFailures(saveError?: Error): ValidationFailures {
  if (!saveError || saveError.status !== 400) {
    return {
      errors: [],
      warnings: [],
    };
  }

  return cloneDeep(saveError.responseJSON as ValidationFailure[]).reduce(
    (acc: ValidationFailures, failure: ValidationFailure) => {
      if (failure.isWarning) {
        acc.warnings.push(failure as ValidationWarning);
      } else {
        acc.errors.push(failure as ValidationError);
      }

      return acc;
    },
    {
      errors: [],
      warnings: [],
    }
  );
}

function getFailures(failures: ValidationFailure[], key: string) {
  const result = [];

  for (let i = failures.length - 1; i >= 0; i--) {
    if (failures[i].propertyName.toLowerCase() === key.toLowerCase()) {
      result.unshift(mapFailure(failures[i]));

      failures.splice(i, 1);
    }
  }

  return result;
}

function mapFailure(failure: ValidationFailure): Failure {
  return {
    errorMessage: failure.errorMessage,
    infoLink: failure.infoLink,
    detailedDescription: failure.detailedDescription,

    // TODO: Remove these renamed properties
    message: failure.errorMessage,
    link: failure.infoLink,
    detailedMessage: failure.detailedDescription,
  };
}

export interface ModelBaseSetting {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  [id: string]: any;
}

function selectSettings<T extends ModelBaseSetting>(
  item: T,
  pendingChanges?: Partial<ModelBaseSetting>,
  saveError?: Error
) {
  const { errors, warnings } = getValidationFailures(saveError);

  // Merge all settings from the item along with pending
  // changes to ensure any settings that were not included
  // with the item are included.
  const allSettings = Object.assign({}, item, pendingChanges);

  const settings = Object.keys(allSettings).reduce(
    (acc: PendingSection<T>, key) => {
      if (key === 'fields') {
        return acc;
      }

      // Return a flattened value
      if (key === 'implementationName') {
        acc.implementationName = item[key];

        return acc;
      }

      const setting: Pending<T> = {
        value: item[key],
        pending: false,
        errors: getFailures(errors, key),
        warnings: getFailures(warnings, key),
      };

      if (pendingChanges?.hasOwnProperty(key)) {
        setting.previousValue = setting.value;
        setting.value = pendingChanges[key];
        setting.pending = true;
      }

      // @ts-expect-error - This is a valid key
      acc[key] = setting;
      return acc;
    },
    {} as PendingSection<T>
  );

  if ('fields' in item) {
    const fields =
      (item.fields as Field[]).reduce((acc: PendingField<T>[], f) => {
        const field: PendingField<T> = Object.assign(
          { pending: false, errors: [], warnings: [] },
          f
        );

        if (pendingChanges && 'fields' in pendingChanges) {
          const pendingChangesFields = pendingChanges.fields as Record<
            string,
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            any
          >;

          if (pendingChangesFields.hasOwnProperty(field.name)) {
            field.previousValue = field.value;
            field.value = pendingChangesFields[field.name];
            field.pending = true;
          }
        }

        field.errors = getFailures(errors, field.name);
        field.warnings = getFailures(warnings, field.name);

        acc.push(field);
        return acc;
      }, []) ?? [];

    if (fields.length) {
      settings.fields = fields;
    }
  }

  const validationErrors = errors;
  const validationWarnings = warnings;

  return {
    settings,
    validationErrors,
    validationWarnings,
    hasPendingChanges: !isEmpty(pendingChanges),
    hasSettings: !isEmpty(settings),
    pendingChanges,
  };
}

export default selectSettings;
