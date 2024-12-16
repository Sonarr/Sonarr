import { push } from 'connected-react-router';
import { ExtendedKeyboardEvent } from 'mousetrap';
import React, {
  FormEvent,
  KeyboardEvent,
  SyntheticEvent,
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import Autosuggest from 'react-autosuggest';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import { Tag } from 'App/State/TagsAppState';
import Icon from 'Components/Icon';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import useDebouncedCallback from 'Helpers/Hooks/useDebouncedCallback';
import useKeyboardShortcuts from 'Helpers/Hooks/useKeyboardShortcuts';
import { icons } from 'Helpers/Props';
import Series from 'Series/Series';
import createAllSeriesSelector from 'Store/Selectors/createAllSeriesSelector';
import createDeepEqualSelector from 'Store/Selectors/createDeepEqualSelector';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import translate from 'Utilities/String/translate';
import SeriesSearchResult from './SeriesSearchResult';
import styles from './SeriesSearchInput.css';

const ADD_NEW_TYPE = 'addNew';

interface Match {
  key: string;
  refIndex: number;
}

interface AddNewSeriesSuggestion {
  type: 'addNew';
  title: string;
}

export interface SuggestedSeries
  extends Pick<
    Series,
    | 'title'
    | 'titleSlug'
    | 'sortTitle'
    | 'images'
    | 'alternateTitles'
    | 'tvdbId'
    | 'tvMazeId'
    | 'imdbId'
    | 'tmdbId'
  > {
  firstCharacter: string;
  tags: Tag[];
}

interface SeriesSuggestion {
  title: string;
  indices: number[];
  item: SuggestedSeries;
  matches: Match[];
  refIndex: number;
}

interface Section {
  title: string;
  loading?: boolean;
  suggestions: SeriesSuggestion[] | AddNewSeriesSuggestion[];
}

function createUnoptimizedSelector() {
  return createSelector(
    createAllSeriesSelector(),
    createTagsSelector(),
    (allSeries, allTags) => {
      return allSeries.map((series): SuggestedSeries => {
        const {
          title,
          titleSlug,
          sortTitle,
          images,
          alternateTitles = [],
          tvdbId,
          tvMazeId,
          imdbId,
          tmdbId,
          tags = [],
        } = series;

        return {
          title,
          titleSlug,
          sortTitle,
          images,
          alternateTitles,
          tvdbId,
          tvMazeId,
          imdbId,
          tmdbId,
          firstCharacter: title.charAt(0).toLowerCase(),
          tags: tags.reduce<Tag[]>((acc, id) => {
            const matchingTag = allTags.find((tag) => tag.id === id);

            if (matchingTag) {
              acc.push(matchingTag);
            }

            return acc;
          }, []),
        };
      });
    }
  );
}

function createSeriesSelector() {
  return createDeepEqualSelector(
    createUnoptimizedSelector(),
    (series) => series
  );
}

function SeriesSearchInput() {
  const series = useSelector(createSeriesSelector());
  const dispatch = useDispatch();
  const { bindShortcut, unbindShortcut } = useKeyboardShortcuts();

  const [value, setValue] = useState('');
  const [requestLoading, setRequestLoading] = useState(false);
  const [suggestions, setSuggestions] = useState<SeriesSuggestion[]>([]);

  const autosuggestRef = useRef<Autosuggest>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  const worker = useRef<Worker | null>(null);
  const isLoading = useRef(false);
  const requestValue = useRef<string | null>(null);

  const suggestionGroups = useMemo(() => {
    const result: Section[] = [];

    if (suggestions.length || isLoading.current) {
      result.push({
        title: translate('ExistingSeries'),
        loading: isLoading.current,
        suggestions,
      });
    }

    result.push({
      title: translate('AddNewSeries'),
      suggestions: [
        {
          type: ADD_NEW_TYPE,
          title: value,
        },
      ],
    });

    return result;
  }, [suggestions, value]);

  const handleSuggestionsReceived = useCallback(
    (message: { data: { value: string; suggestions: SeriesSuggestion[] } }) => {
      const { value, suggestions } = message.data;

      if (!isLoading.current) {
        requestValue.current = null;
        setRequestLoading(false);
      } else if (value === requestValue.current) {
        setSuggestions(suggestions);
        requestValue.current = null;
        setRequestLoading(false);
        isLoading.current = false;
        // setLoading(false);
      } else {
        setSuggestions(suggestions);
        setRequestLoading(true);

        const payload = {
          value: requestValue,
          series,
        };

        worker.current?.postMessage(payload);
      }
    },
    [series]
  );

  const requestSuggestions = useDebouncedCallback((value: string) => {
    if (!isLoading.current) {
      return;
    }

    requestValue.current = value;
    setRequestLoading(true);

    if (!requestLoading) {
      const payload = {
        value,
        series,
      };

      worker.current?.postMessage(payload);
    }
  }, 250);

  const reset = useCallback(() => {
    setValue('');
    setSuggestions([]);
    // setLoading(false);
    isLoading.current = false;
  }, []);

  const focusInput = useCallback((event: ExtendedKeyboardEvent) => {
    event.preventDefault();
    inputRef.current?.focus();
  }, []);

  const getSectionSuggestions = useCallback((section: Section) => {
    return section.suggestions;
  }, []);

  const renderSectionTitle = useCallback((section: Section) => {
    return (
      <div className={styles.sectionTitle}>
        {section.title}

        {section.loading && (
          <LoadingIndicator
            className={styles.loading}
            rippleClassName={styles.ripple}
            size={20}
          />
        )}
      </div>
    );
  }, []);

  const getSuggestionValue = useCallback(({ title }: { title: string }) => {
    return title;
  }, []);

  const renderSuggestion = useCallback(
    (
      item: AddNewSeriesSuggestion | SeriesSuggestion,
      { query }: { query: string }
    ) => {
      if ('type' in item) {
        return (
          <div className={styles.addNewSeriesSuggestion}>
            {translate('SearchForQuery', { query })}
          </div>
        );
      }

      return <SeriesSearchResult {...item.item} match={item.matches[0]} />;
    },
    []
  );

  const handleChange = useCallback(
    (
      _event: FormEvent<HTMLElement>,
      {
        newValue,
        method,
      }: {
        newValue: string;
        method: 'down' | 'up' | 'escape' | 'enter' | 'click' | 'type';
      }
    ) => {
      if (method === 'up' || method === 'down') {
        return;
      }

      setValue(newValue);
    },
    []
  );

  const handleKeyDown = useCallback(
    (event: KeyboardEvent<HTMLElement>) => {
      if (event.shiftKey || event.altKey || event.ctrlKey) {
        return;
      }

      if (event.key === 'Escape') {
        reset();
        return;
      }

      if (event.key !== 'Tab' && event.key !== 'Enter') {
        return;
      }

      if (!autosuggestRef.current) {
        return;
      }

      const { highlightedSectionIndex, highlightedSuggestionIndex } =
        autosuggestRef.current.state;

      if (!suggestions.length || highlightedSectionIndex) {
        dispatch(
          push(
            `${window.Sonarr.urlBase}/add/new?term=${encodeURIComponent(value)}`
          )
        );

        inputRef.current?.blur();
        reset();

        return;
      }

      // If an suggestion is not selected go to the first series,
      // otherwise go to the selected series.

      const selectedSuggestion =
        highlightedSuggestionIndex == null
          ? suggestions[0]
          : suggestions[highlightedSuggestionIndex];

      dispatch(
        push(
          `${window.Sonarr.urlBase}/series/${selectedSuggestion.item.titleSlug}`
        )
      );

      inputRef.current?.blur();
      reset();
    },
    [value, suggestions, dispatch, reset]
  );

  const handleBlur = useCallback(() => {
    reset();
  }, [reset]);

  const handleSuggestionsFetchRequested = useCallback(
    ({ value }: { value: string }) => {
      isLoading.current = true;

      requestSuggestions(value);
    },
    [requestSuggestions]
  );

  const handleSuggestionsClearRequested = useCallback(() => {
    setSuggestions([]);
    isLoading.current = false;
  }, []);

  const handleSuggestionSelected = useCallback(
    (
      _event: SyntheticEvent,
      { suggestion }: { suggestion: SeriesSuggestion | AddNewSeriesSuggestion }
    ) => {
      if ('type' in suggestion) {
        dispatch(
          push(
            `${window.Sonarr.urlBase}/add/new?term=${encodeURIComponent(value)}`
          )
        );
      } else {
        setValue('');
        dispatch(
          push(`${window.Sonarr.urlBase}/series/${suggestion.item.titleSlug}`)
        );
      }
    },
    [value, dispatch]
  );

  const inputProps = {
    ref: inputRef,
    className: styles.input,
    name: 'seriesSearch',
    value,
    placeholder: translate('Search'),
    autoComplete: 'off',
    spellCheck: false,
    onChange: handleChange,
    onKeyDown: handleKeyDown,
    onBlur: handleBlur,
  };

  const theme = {
    container: styles.container,
    containerOpen: styles.containerOpen,
    suggestionsContainer: styles.seriesContainer,
    suggestionsList: styles.list,
    suggestion: styles.listItem,
    suggestionHighlighted: styles.highlighted,
  };

  useEffect(() => {
    worker.current = new Worker(new URL('./fuse.worker.ts', import.meta.url));

    return () => {
      if (worker.current) {
        worker.current.terminate();
        worker.current = null;
      }
    };
  }, []);

  useEffect(() => {
    worker.current?.addEventListener(
      'message',
      handleSuggestionsReceived,
      false
    );

    return () => {
      if (worker.current) {
        worker.current.removeEventListener(
          'message',
          handleSuggestionsReceived,
          false
        );
      }
    };
  }, [handleSuggestionsReceived]);

  useEffect(() => {
    bindShortcut('focusSeriesSearchInput', focusInput);

    return () => {
      unbindShortcut('focusSeriesSearchInput');
    };
  }, [bindShortcut, unbindShortcut, focusInput]);

  return (
    <div className={styles.wrapper}>
      <Icon name={icons.SEARCH} />

      <Autosuggest
        ref={autosuggestRef}
        inputProps={inputProps}
        theme={theme}
        focusInputOnSuggestionClick={false}
        multiSection={true}
        suggestions={suggestionGroups}
        getSectionSuggestions={getSectionSuggestions}
        renderSectionTitle={renderSectionTitle}
        getSuggestionValue={getSuggestionValue}
        renderSuggestion={renderSuggestion}
        onSuggestionSelected={handleSuggestionSelected}
        onSuggestionsFetchRequested={handleSuggestionsFetchRequested}
        onSuggestionsClearRequested={handleSuggestionsClearRequested}
      />
    </div>
  );
}

export default SeriesSearchInput;
