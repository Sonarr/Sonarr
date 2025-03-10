import {
  autoUpdate,
  flip,
  FloatingPortal,
  useClick,
  useDismiss,
  useFloating,
  useInteractions,
} from '@floating-ui/react';
import React, { useCallback, useEffect, useRef, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import FormInputButton from 'Components/Form/FormInputButton';
import TextInput from 'Components/Form/TextInput';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
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

  const seriesLookupTimeout = useRef<ReturnType<typeof setTimeout>>();

  const [term, setTerm] = useState('');
  const [isOpen, setIsOpen] = useState(false);

  const errorMessage = getErrorMessage(error);

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
    setTerm(itemTerm);
  }, [itemTerm]);

  const { refs, context, floatingStyles } = useFloating({
    middleware: [
      flip({
        crossAxis: false,
        mainAxis: true,
      }),
    ],
    open: isOpen,
    placement: 'bottom',
    whileElementsMounted: autoUpdate,
    onOpenChange: setIsOpen,
  });

  const click = useClick(context);
  const dismiss = useDismiss(context);

  const { getReferenceProps, getFloatingProps } = useInteractions([
    click,
    dismiss,
  ]);

  return (
    <>
      <div ref={refs.setReference} {...getReferenceProps()}>
        <Link className={styles.button} component="div" onPress={handlePress}>
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
      {isOpen ? (
        <FloatingPortal id="portal-root">
          <div
            ref={refs.setFloating}
            className={styles.contentContainer}
            style={floatingStyles}
            {...getFloatingProps()}
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
        </FloatingPortal>
      ) : null}
    </>
  );
}

export default ImportSeriesSelectSeries;
