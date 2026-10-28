import {
  useFloating,
  useInteractions,
  useListNavigation,
  useRole,
} from '@floating-ui/react';
import classNames from 'classnames';
import { ExtendedKeyboardEvent } from 'mousetrap';
import React, {
  ChangeEvent,
  KeyboardEvent,
  MouseEvent,
  TouchEvent,
  useCallback,
  useEffect,
  useId,
  useMemo,
  useRef,
  useState,
} from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useDebouncedCallback } from 'use-debounce';
import Icon from 'Components/Icon';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import useKeyboardShortcuts from 'Helpers/Hooks/useKeyboardShortcuts';
import { icons } from 'Helpers/Props';
import Series from 'Series/Series';
import useSeries from 'Series/useSeries';
import { Tag, useTagList } from 'Tags/useTags';
import translate from 'Utilities/String/translate';
import SeriesSearchResult from './SeriesSearchResult';
import styles from './SeriesSearchInput.module.css';

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
    | 'translations'
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

function isModifiedClick(event: MouseEvent) {
  return event.metaKey || event.ctrlKey || event.shiftKey || event.altKey;
}

function useSeriesSuggestions(tagList: ReadonlyArray<Tag>) {
  const { data: allSeries = [] } = useSeries();

  return useMemo(() => {
    return allSeries.map((series): SuggestedSeries => {
      const {
        title,
        titleSlug,
        sortTitle,
        images,
        alternateTitles = [],
        translations = [],
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
        translations,
        tvdbId,
        tvMazeId,
        imdbId,
        tmdbId,
        firstCharacter: title.charAt(0).toLowerCase(),
        tags: tags.reduce<Tag[]>((acc, id) => {
          const matchingTag = tagList.find((tag) => tag.id === id);

          if (matchingTag) {
            acc.push(matchingTag);
          }

          return acc;
        }, []),
      };
    });
  }, [allSeries, tagList]);
}

function SeriesSearchInput() {
  const tagList = useTagList();
  const series = useSeriesSuggestions(tagList);
  const navigate = useNavigate();
  const { bindShortcut, unbindShortcut } = useKeyboardShortcuts();
  const sectionId = useId();

  const [value, setValue] = useState('');
  const [requestLoading, setRequestLoading] = useState(false);
  const [suggestions, setSuggestions] = useState<SeriesSuggestion[]>([]);
  const [activeIndex, setActiveIndex] = useState<number | null>(null);

  const inputRef = useRef<HTMLInputElement | null>(null);
  const listRef = useRef<Array<HTMLElement | null>>([]);
  const worker = useRef<Worker | null>(null);
  const isLoading = useRef(false);
  const requestValue = useRef<string | null>(null);

  const isOpen = value.trim().length > 0;

  const { refs, context } = useFloating({
    open: isOpen,
  });

  const listNavigation = useListNavigation(context, {
    listRef,
    activeIndex,
    virtual: true,
    loop: true,
    onNavigate: setActiveIndex,
  });

  const role = useRole(context, { role: 'combobox' });

  const { getReferenceProps, getFloatingProps, getItemProps } = useInteractions(
    [listNavigation, role]
  );

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

  const addNewPath = `/add/new?term=${encodeURIComponent(value)}`;

  const handleSuggestionsReceived = useCallback(
    (message: { data: { value: string; suggestions: SeriesSuggestion[] } }) => {
      const { value, suggestions } = message.data;

      if (!isLoading.current) {
        requestValue.current = null;
        setRequestLoading(false);
      } else if (value === requestValue.current) {
        setSuggestions(suggestions);
        setActiveIndex(null);
        requestValue.current = null;
        setRequestLoading(false);
        isLoading.current = false;
      } else {
        setSuggestions(suggestions);
        setActiveIndex(null);
        setRequestLoading(true);

        worker.current?.postMessage({ value: requestValue.current });
      }
    },
    []
  );

  const requestSuggestions = useDebouncedCallback((value: string) => {
    if (!isLoading.current) {
      return;
    }

    requestValue.current = value;
    setRequestLoading(true);

    if (!requestLoading) {
      worker.current?.postMessage({ value });
    }
  }, 250);

  const reset = useCallback(() => {
    setValue('');
    setSuggestions([]);
    setActiveIndex(null);
    isLoading.current = false;
  }, []);

  const focusInput = useCallback((event: ExtendedKeyboardEvent) => {
    event.preventDefault();
    inputRef.current?.focus();
  }, []);

  const handleChange = useCallback(
    (event: ChangeEvent<HTMLInputElement>) => {
      const newValue = event.target.value;

      setValue(newValue);
      setActiveIndex(null);

      if (newValue.trim().length) {
        isLoading.current = true;
        requestSuggestions(newValue);
      } else {
        setSuggestions([]);
        isLoading.current = false;
      }
    },
    [requestSuggestions]
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

      if (!inputRef.current?.value) {
        return;
      }

      if (!suggestions.length || activeIndex === suggestions.length) {
        navigate(addNewPath);

        inputRef.current?.blur();
        reset();

        return;
      }

      // If a suggestion is not selected go to the first series,
      // otherwise go to the selected series.

      const selectedSuggestion =
        activeIndex == null ? suggestions[0] : suggestions[activeIndex];

      navigate(`/series/${selectedSuggestion.item.titleSlug}`);

      inputRef.current?.blur();
      reset();
    },
    [activeIndex, addNewPath, suggestions, navigate, reset]
  );

  const handleBlur = useCallback(() => {
    reset();
  }, [reset]);

  const handleTouchEnd = useCallback((event: TouchEvent<HTMLInputElement>) => {
    const input = event.currentTarget;

    if (!event.cancelable || document.activeElement === input) {
      return;
    }

    event.preventDefault();
    input.focus({ preventScroll: true });
  }, []);

  const handleDropdownMouseDown = useCallback((event: MouseEvent) => {
    event.preventDefault();
  }, []);

  const handleSuggestionClick = useCallback(
    (event: MouseEvent) => {
      if (isModifiedClick(event)) {
        return;
      }

      inputRef.current?.blur();
      reset();
    },
    [reset]
  );

  const setInputRef = useCallback(
    (node: HTMLInputElement | null) => {
      inputRef.current = node;
      refs.setReference(node);
    },
    [refs]
  );

  useEffect(() => {
    worker.current = new Worker(new URL('./fuse.worker.ts', import.meta.url), {
      type: 'module',
    });

    return () => {
      if (worker.current) {
        worker.current.terminate();
        worker.current = null;
      }
    };
  }, []);

  useEffect(() => {
    worker.current?.postMessage({ series });
  }, [series]);

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

  let itemIndex = 0;

  return (
    <div className={styles.wrapper}>
      <div className={styles.inputWrapper}>
        <Icon className={styles.searchIcon} name={icons.SEARCH} />

        <div className={styles.container}>
          <input
            {...getReferenceProps({
              ref: setInputRef,
              className: styles.input,
              type: 'text',
              name: 'seriesSearch',
              value,
              placeholder: translate('Search'),
              autoComplete: 'off',
              spellCheck: false,
              onChange: handleChange,
              onKeyDown: handleKeyDown,
              onBlur: handleBlur,
              onTouchEnd: handleTouchEnd,
            })}
          />

          {isOpen ? (
            <div
              className={styles.dropdown}
              onMouseDown={handleDropdownMouseDown}
            >
              <div
                {...getFloatingProps({
                  ref: refs.setFloating,
                  className: styles.seriesContainer,
                })}
              >
                {suggestionGroups.map((section, sectionIndex) => {
                  const titleId = `${sectionId}-${sectionIndex}`;

                  return (
                    <div
                      key={section.title}
                      role="group"
                      aria-labelledby={titleId}
                    >
                      <div id={titleId} className={styles.sectionTitle}>
                        {section.title}

                        {section.loading && (
                          <LoadingIndicator
                            className={styles.loading}
                            rippleClassName={styles.ripple}
                            size={12}
                          />
                        )}
                      </div>

                      {section.suggestions.map((item) => {
                        const index = itemIndex++;
                        const isActive = index === activeIndex;
                        const isAddNew = 'type' in item;

                        return (
                          <Link
                            key={isAddNew ? ADD_NEW_TYPE : item.item.titleSlug}
                            {...getItemProps({
                              ref: (node: HTMLAnchorElement | null) => {
                                listRef.current[index] = node;
                              },
                              active: isActive,
                              selected: isActive,
                            })}
                            className={classNames(
                              styles.listItem,
                              isActive && styles.highlighted,
                              isAddNew && styles.addNewSeriesSuggestion
                            )}
                            to={
                              isAddNew
                                ? addNewPath
                                : `/series/${item.item.titleSlug}`
                            }
                            tabIndex={-1}
                            onClick={handleSuggestionClick}
                          >
                            {isAddNew ? (
                              translate('SearchForQuery', { query: value })
                            ) : (
                              <SeriesSearchResult
                                {...item.item}
                                match={item.matches[0]}
                              />
                            )}
                          </Link>
                        );
                      })}
                    </div>
                  );
                })}
              </div>
            </div>
          ) : null}
        </div>
      </div>
    </div>
  );
}

export default SeriesSearchInput;
