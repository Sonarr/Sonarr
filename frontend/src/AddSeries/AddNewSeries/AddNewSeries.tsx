import React, { useCallback, useEffect, useRef, useState } from 'react';
import TextInput from 'Components/Form/TextInput';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import Link from 'Components/Link/Link';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageHeading from 'Components/Page/PageHeading';
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
  const searchInputRef = useRef<HTMLInputElement>(null);
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
    searchInputRef.current?.focus();
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
        <PageHeading
          scope={translate('Media')}
          title={translate('AddNewSeries')}
        />

        <div className={styles.searchSticky}>
          <div className={styles.searchWrap}>
            <Icon className={styles.searchIcon} name={icons.SEARCH} size={18} />

            <TextInput
              ref={searchInputRef}
              className={styles.searchInput}
              name="seriesLookup"
              value={term}
              placeholder="eg. Breaking Bad, tvdb:####"
              autoFocus={true}
              onChange={handleSearchInputChange}
            />

            {term ? (
              <Button
                className={styles.clearLookupButton}
                onPress={handleClearSeriesLookupPress}
              >
                <Icon name={icons.REMOVE} size={14} />
              </Button>
            ) : null}
          </div>
        </div>

        {isFetching ? <LoadingIndicator /> : null}

        {!isFetching && !!error ? (
          <div className={styles.emptyState}>
            <div className={styles.emptyTitle}>
              {translate('AddNewSeriesError')}
            </div>
            <p className={styles.error}>{getErrorMessage(error)}</p>
          </div>
        ) : null}

        {!isFetching && !error && !!data.length ? (
          <div className={styles.searchResults}>
            <div className={styles.resultsLabel}>
              {data.length === 1
                ? translate('CountResult', { count: data.length })
                : translate('CountResults', { count: data.length })}
            </div>

            {data.map((item) => (
              <AddNewSeriesSearchResult key={item.tvdbId} series={item} />
            ))}
          </div>
        ) : null}

        {!isFetching && !error && !data.length && term ? (
          <div className={styles.emptyState}>
            <div className={styles.emptyTitle}>
              {translate('CouldNotFindResults', { term })}
            </div>
            <div className={styles.emptyBody}>
              {translate('SearchByTvdbId')}
            </div>
            <div>
              <Link
                className={styles.emptyLink}
                to="https://wiki.servarr.com/sonarr/faq#why-cant-i-add-a-new-series-when-i-know-the-tvdb-id"
              >
                {translate('WhyCantIFindMyShow')}
              </Link>
            </div>
          </div>
        ) : null}

        {term ? null : (
          <div className={styles.emptyState}>
            <div className={styles.emptyTitle}>
              {translate('AddNewSeriesHelpText')}
            </div>
            <div className={styles.emptyBody}>
              {translate('SearchByTvdbId')}
            </div>

            {hasSeries ? null : (
              <>
                <div className={styles.emptyDivider} />
                <div className={styles.emptyNoLibrary}>
                  {translate('NoSeriesHaveBeenAdded')}
                </div>
                <Button to="/add/import" kind={kinds.PRIMARY}>
                  {translate('ImportExistingSeries')}
                </Button>
              </>
            )}
          </div>
        )}
      </PageContentBody>
    </PageContent>
  );
}

export default AddNewSeries;
