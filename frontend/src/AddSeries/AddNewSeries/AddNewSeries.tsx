import React, { useCallback, useEffect, useState } from 'react';
import Alert from 'Components/Alert';
import TextInput from 'Components/Form/TextInput';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import Link from 'Components/Link/Link';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import useDebounce from 'Helpers/Hooks/useDebounce';
import useQueryParams from 'Helpers/Hooks/useQueryParams';
import { icons, kinds } from 'Helpers/Props';
import { useHasSeries } from 'Series/useSeries';
import { InputChanged } from 'typings/inputs';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import translate from 'Utilities/String/translate';
import AddNewSeriesSearchResult from './AddNewSeriesSearchResult';
import { useLookupSeries } from './useAddSeries';
import styles from './AddNewSeries.css';

function AddNewSeries() {
  const { term: initialTerm = '' } = useQueryParams<{ term: string }>();
  const hasSeries = useHasSeries();
  const [term, setTerm] = useState(initialTerm);
  const [isFetching, setIsFetching] = useState(false);
  const query = useDebounce(term, term ? 300 : 0);

  const handleSearchInputChange = useCallback(
    ({ value }: InputChanged<string>) => {
      setTerm(value);
      setIsFetching(!!value.trim());
    },
    []
  );

  const handleClearSeriesLookupPress = useCallback(() => {
    setTerm('');
    setIsFetching(false);
  }, []);

  const { isFetching: isFetchingApi, error, data } = useLookupSeries(query);

  useEffect(() => {
    setIsFetching(isFetchingApi);
  }, [isFetchingApi]);

  useEffect(() => {
    setTerm(initialTerm);
  }, [initialTerm]);

  return (
    <PageContent title={translate('AddNewSeries')}>
      <PageContentBody>
        <div className={styles.searchContainer}>
          <div className={styles.searchIconContainer}>
            <Icon name={icons.SEARCH} size={20} />
          </div>

          <TextInput
            className={styles.searchInput}
            name="seriesLookup"
            value={term}
            placeholder="eg. Breaking Bad, tvdb:####"
            autoFocus={true}
            onChange={handleSearchInputChange}
          />

          <Button
            className={styles.clearLookupButton}
            onPress={handleClearSeriesLookupPress}
          >
            <Icon name={icons.REMOVE} size={20} />
          </Button>
        </div>

        {isFetching ? <LoadingIndicator /> : null}

        {!isFetching && !!error ? (
          <div className={styles.message}>
            <div className={styles.helpText}>
              {translate('AddNewSeriesError')}
            </div>

            <Alert kind={kinds.DANGER}>{getErrorMessage(error)}</Alert>
          </div>
        ) : null}

        {!isFetching && !error && !!data.length ? (
          <div className={styles.searchResults}>
            {data.map((item) => {
              return (
                <AddNewSeriesSearchResult key={item.tvdbId} series={item} />
              );
            })}
          </div>
        ) : null}

        {!isFetching && !error && !data.length && term ? (
          <div className={styles.message}>
            <div className={styles.noResults}>
              {translate('CouldNotFindResults', { term })}
            </div>
            <div>{translate('SearchByTvdbId')}</div>
            <div>
              <Link to="https://wiki.servarr.com/sonarr/faq#why-cant-i-add-a-new-series-when-i-know-the-tvdb-id">
                {translate('WhyCantIFindMyShow')}
              </Link>
            </div>
          </div>
        ) : null}

        {term ? null : (
          <div className={styles.message}>
            <div className={styles.helpText}>
              {translate('AddNewSeriesHelpText')}
            </div>
            <div>{translate('SearchByTvdbId')}</div>
          </div>
        )}

        {!term && !hasSeries ? (
          <div className={styles.message}>
            <div className={styles.noSeriesText}>
              {translate('NoSeriesHaveBeenAdded')}
            </div>
            <div>
              <Button to="/add/import" kind={kinds.PRIMARY}>
                {translate('ImportExistingSeries')}
              </Button>
            </div>
          </div>
        ) : null}

        <div />
      </PageContentBody>
    </PageContent>
  );
}

export default AddNewSeries;
