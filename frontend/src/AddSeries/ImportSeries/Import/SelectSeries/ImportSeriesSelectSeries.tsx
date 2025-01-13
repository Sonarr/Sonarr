import React, { useCallback, useEffect, useId, useRef, useState } from 'react';
import { Manager, Popper, Reference } from 'react-popper';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import FormInputButton from 'Components/Form/FormInputButton';
import TextInput from 'Components/Form/TextInput';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Portal from 'Components/Portal';
import { icons, kinds } from 'Helpers/Props';
import {
  queueLookupSeries,
  setImportSeriesValue,
} from 'Store/Actions/importSeriesActions';
import createImportSeriesItemSelector from 'Store/Selectors/createImportSeriesItemSelector';
import { InputChanged } from 'typings/inputs';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import translate from 'Utilities/String/translate';
import ImportSeriesSearchResult from './ImportSeriesSearchResult';
import ImportSeriesTitle from './ImportSeriesTitle';
import styles from './ImportSeriesSelectSeries.css';

interface ImportSeriesSelectSeriesProps {
  id: string;
  onInputChange: (input: InputChanged) => void;
}

function ImportSeriesSelectSeries({
  id,
  onInputChange,
}: ImportSeriesSelectSeriesProps) {
  const dispatch = useDispatch();
  const isLookingUpSeries = useSelector(
    (state: AppState) => state.importSeries.isLookingUpSeries
  );

  const {
    error,
    isFetching = true,
    isPopulated = false,
    items = [],
    isQueued = true,
    selectedSeries,
    isExistingSeries,
    term: itemTerm,
    // @ts-expect-error - ignoring this for now
  } = useSelector(createImportSeriesItemSelector(id, { id }));

  const buttonId = useId();
  const contentId = useId();
  const updater = useRef<(() => void) | null>(null);
  const seriesLookupTimeout = useRef<ReturnType<typeof setTimeout>>();

  const [term, setTerm] = useState('');
  const [isOpen, setIsOpen] = useState(false);

  const errorMessage = getErrorMessage(error);

  const handleWindowClick = useCallback(
    (event: MouseEvent) => {
      const button = document.getElementById(buttonId);
      const content = document.getElementById(contentId);
      const eventTarget = event.target as HTMLElement;

      if (!button || !eventTarget.isConnected) {
        return;
      }

      if (
        !button.contains(eventTarget) &&
        content &&
        !content.contains(eventTarget) &&
        isOpen
      ) {
        setIsOpen(false);
        window.removeEventListener('click', handleWindowClick);
      }
    },
    [isOpen, buttonId, contentId, setIsOpen]
  );

  const addListener = useCallback(() => {
    window.addEventListener('click', handleWindowClick);
  }, [handleWindowClick]);

  const removeListener = useCallback(() => {
    window.removeEventListener('click', handleWindowClick);
  }, [handleWindowClick]);

  const handlePress = useCallback(() => {
    setIsOpen((prevIsOpen) => !prevIsOpen);
  }, []);

  const handleSearchInputChange = useCallback(
    ({ value }: InputChanged<string>) => {
      if (seriesLookupTimeout.current) {
        clearTimeout(seriesLookupTimeout.current);
      }

      setTerm(value);

      seriesLookupTimeout.current = setTimeout(() => {
        dispatch(
          queueLookupSeries({
            name: id,
            term: value,
            topOfQueue: true,
          })
        );
      }, 200);
    },
    [id, dispatch]
  );

  const handleRefreshPress = useCallback(() => {
    dispatch(
      queueLookupSeries({
        name: id,
        term,
        topOfQueue: true,
      })
    );
  }, [id, term, dispatch]);

  const handleSeriesSelect = useCallback(
    (tvdbId: number) => {
      setIsOpen(false);

      const selectedSeries = items.find((item) => item.tvdbId === tvdbId)!;

      dispatch(
        // @ts-expect-error - actions are not typed
        setImportSeriesValue({
          id,
          selectedSeries,
        })
      );

      if (selectedSeries.seriesType !== 'standard') {
        onInputChange({
          name: 'seriesType',
          value: selectedSeries.seriesType,
        });
      }
    },
    [id, items, dispatch, onInputChange]
  );

  useEffect(() => {
    if (updater.current) {
      updater.current();
    }
  });

  useEffect(() => {
    if (isOpen) {
      addListener();
    } else {
      removeListener();
    }

    return removeListener;
  }, [isOpen, addListener, removeListener]);

  useEffect(() => {
    setTerm(itemTerm);
  }, [itemTerm]);

  return (
    <Manager>
      <Reference>
        {({ ref }) => (
          <div ref={ref} id={buttonId}>
            <Link
              // ref={ref}
              className={styles.button}
              component="div"
              onPress={handlePress}
            >
              {isLookingUpSeries && isQueued && !isPopulated ? (
                <LoadingIndicator className={styles.loading} size={20} />
              ) : null}

              {isPopulated && selectedSeries && isExistingSeries ? (
                <Icon
                  className={styles.warningIcon}
                  name={icons.WARNING}
                  kind={kinds.WARNING}
                />
              ) : null}

              {isPopulated && selectedSeries ? (
                <ImportSeriesTitle
                  title={selectedSeries.title}
                  year={selectedSeries.year}
                  network={selectedSeries.network}
                  isExistingSeries={isExistingSeries}
                />
              ) : null}

              {isPopulated && !selectedSeries ? (
                <div>
                  <Icon
                    className={styles.warningIcon}
                    name={icons.WARNING}
                    kind={kinds.WARNING}
                  />

                  {translate('NoMatchFound')}
                </div>
              ) : null}

              {!isFetching && !!error ? (
                <div>
                  <Icon
                    className={styles.warningIcon}
                    title={errorMessage}
                    name={icons.WARNING}
                    kind={kinds.WARNING}
                  />

                  {translate('SearchFailedError')}
                </div>
              ) : null}

              <div className={styles.dropdownArrowContainer}>
                <Icon name={icons.CARET_DOWN} />
              </div>
            </Link>
          </div>
        )}
      </Reference>

      <Portal>
        <Popper
          placement="bottom"
          modifiers={{
            preventOverflow: {
              boundariesElement: 'viewport',
            },
          }}
        >
          {({ ref, style, scheduleUpdate }) => {
            updater.current = scheduleUpdate;

            return (
              <div
                ref={ref}
                id={contentId}
                className={styles.contentContainer}
                style={style}
              >
                {isOpen ? (
                  <div className={styles.content}>
                    <div className={styles.searchContainer}>
                      <div className={styles.searchIconContainer}>
                        <Icon name={icons.SEARCH} />
                      </div>

                      <TextInput
                        className={styles.searchInput}
                        name={`${name}_textInput`}
                        value={term}
                        onChange={handleSearchInputChange}
                      />

                      <FormInputButton
                        kind={kinds.DEFAULT}
                        spinnerIcon={icons.REFRESH}
                        canSpin={true}
                        isSpinning={isFetching}
                        onPress={handleRefreshPress}
                      >
                        <Icon name={icons.REFRESH} />
                      </FormInputButton>
                    </div>

                    <div className={styles.results}>
                      {items.map((item) => {
                        return (
                          <ImportSeriesSearchResult
                            key={item.tvdbId}
                            tvdbId={item.tvdbId}
                            title={item.title}
                            year={item.year}
                            network={item.network}
                            onPress={handleSeriesSelect}
                          />
                        );
                      })}
                    </div>
                  </div>
                ) : null}
              </div>
            );
          }}
        </Popper>
      </Portal>
    </Manager>
  );
}

export default ImportSeriesSelectSeries;
