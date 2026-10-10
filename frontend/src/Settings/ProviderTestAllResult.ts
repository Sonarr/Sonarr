import { ValidationFailure } from 'typings/pending';
import { ApiError } from 'Utilities/Fetch/fetchJson';
import translate from 'Utilities/String/translate';

export interface ProviderTestAllResult {
  id: number;
  isValid: boolean;
  validationFailures: ValidationFailure[];
}

export type ProviderTestStatus = 'passed' | 'warning' | 'failed' | 'notTested';

export function getProviderTestAllResults(error: ApiError | null) {
  if (error?.statusCode !== 400 || !Array.isArray(error.statusBody)) {
    return undefined;
  }

  return error.statusBody as unknown as ProviderTestAllResult[];
}

export function getProviderTestStatus(
  result: ProviderTestAllResult | undefined
): ProviderTestStatus {
  if (!result) {
    return 'notTested';
  }

  if (result.validationFailures.some((failure) => !failure.isWarning)) {
    return 'failed';
  }

  if (result.validationFailures.some((failure) => failure.isWarning)) {
    return 'warning';
  }

  return 'passed';
}

export function getProviderTestStatusLabel(status: ProviderTestStatus) {
  switch (status) {
    case 'passed':
      return translate('ProviderTestPassed');
    case 'warning':
      return translate('ProviderTestWarningStatus');
    case 'failed':
      return translate('ProviderTestFailedStatus');
    default:
      return translate('ProviderNotTested');
  }
}
