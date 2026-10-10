import classNames from 'classnames';
import React, { MouseEvent, ReactNode, useCallback } from 'react';
import { useHref } from 'react-router-dom';
import styles from './SeriesSearchSuggestionLink.module.css';

interface SeriesSearchSuggestionLinkProps {
  className?: string;
  to: string;
  children: ReactNode;
}

function isNewTabClick(event: MouseEvent) {
  return event.button !== 0 || event.ctrlKey || event.metaKey || event.shiftKey;
}

// Renders a suggestion as a real link so it can be opened in a new tab with
// middle click or ctrl/cmd/shift + click. A plain left click is still handled
// by react-autosuggest, which selects the suggestion and navigates in-app.
function SeriesSearchSuggestionLink({
  className,
  to,
  children,
}: SeriesSearchSuggestionLinkProps) {
  const href = useHref(to);

  const handleMouseDown = useCallback((event: MouseEvent) => {
    if (isNewTabClick(event)) {
      // Keep focus in the search input so the suggestions stay open and
      // don't let react-autosuggest treat this as a selection.
      event.preventDefault();
      event.stopPropagation();
    }
  }, []);

  const handleClick = useCallback((event: MouseEvent) => {
    if (isNewTabClick(event)) {
      // Let the browser open the link in a new tab or window.
      event.stopPropagation();
    } else {
      event.preventDefault();
    }
  }, []);

  return (
    <a
      className={classNames(styles.link, className)}
      href={href}
      onMouseDown={handleMouseDown}
      onClick={handleClick}
    >
      {children}
    </a>
  );
}

export default SeriesSearchSuggestionLink;
