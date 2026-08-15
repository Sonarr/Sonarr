import React, { useMemo } from 'react';
import Alert from 'Components/Alert';
import { kinds } from 'Helpers/Props';
import {
  getProviderTestStatus,
  ProviderTestAllResult,
} from 'Settings/ProviderTestAllResult';
import { ApiError } from 'Utilities/Fetch/fetchJson';
import translate from 'Utilities/String/translate';
import { IndexerModel } from '../useIndexers';
import styles from './IndexerTestAllResults.css';

interface IndexerTestAllResultsProps {
  indexers: IndexerModel[];
  results?: ProviderTestAllResult[];
  error?: ApiError | null;
}

function IndexerTestAllResults({
  indexers,
  results,
  error,
}: IndexerTestAllResultsProps) {
  const indexerById = useMemo(
    () => new Map(indexers.map((indexer) => [indexer.id, indexer])),
    [indexers]
  );

  if (!results && !error) {
    return null;
  }

  if (!results) {
    return (
      <div className={styles.results}>
        <Alert kind={kinds.DANGER}>
          <div role="alert">{translate('IndexerTestAllFailed')}</div>
        </Alert>
      </div>
    );
  }

  let passedCount = 0;
  let warningCount = 0;
  let failedCount = 0;

  results.forEach((result) => {
    const status = getProviderTestStatus(result);

    if (status === 'passed') {
      passedCount++;
    } else if (status === 'warning') {
      warningCount++;
    } else {
      failedCount++;
    }
  });

  const notTestedCount = Math.max(indexers.length - results.length, 0);
  const issueResults = results.filter(
    (result) => getProviderTestStatus(result) !== 'passed'
  );
  let kind: 'success' | 'danger' | 'warning' | 'info' = kinds.SUCCESS;

  if (failedCount) {
    kind = kinds.DANGER;
  } else if (warningCount) {
    kind = kinds.WARNING;
  } else if (notTestedCount) {
    kind = kinds.INFO;
  }

  return (
    <div className={styles.results}>
      <Alert kind={kind}>
        <div role="status" aria-atomic={true}>
          {translate('IndexerTestAllSummary', {
            passed: passedCount,
            warnings: warningCount,
            failed: failedCount,
            notTested: notTestedCount,
          })}
        </div>

        {notTestedCount ? (
          <div>{translate('IndexerTestAllNotTestedHelpText')}</div>
        ) : null}

        {issueResults.length ? (
          <ul className={styles.failures}>
            {issueResults.flatMap((result) => {
              const indexerName =
                indexerById.get(result.id)?.name ?? translate('Unknown');

              return result.validationFailures.map((failure, index) => (
                <li key={`${result.id}-${index}`}>
                  {indexerName}: {failure.errorMessage}
                </li>
              ));
            })}
          </ul>
        ) : null}
      </Alert>
    </div>
  );
}

export default IndexerTestAllResults;
