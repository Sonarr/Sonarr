import React from 'react';
import Link, { LinkProps } from 'Components/Link/Link';
import getSeriesPath from 'Utilities/Series/getSeriesPath';

export interface SeriesTitleLinkProps extends LinkProps {
  seriesId: number;
  titleSlug: string;
  title: string;
}

export default function SeriesTitleLink({
  seriesId,
  titleSlug,
  title,
  ...linkProps
}: SeriesTitleLinkProps) {
  const link = getSeriesPath({ id: seriesId, titleSlug });

  return (
    <Link to={link} {...linkProps}>
      {title}
    </Link>
  );
}
