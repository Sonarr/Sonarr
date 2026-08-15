import { ValidationFailure } from 'typings/pending';
import { ApiError } from 'Utilities/Fetch/fetchJson';

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
