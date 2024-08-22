import React from 'react';
import Link, { LinkProps } from 'Components/Link/Link';

export interface SeriesTitleLinkProps extends LinkProps {
  titleSlug: string;
  title: string;
}

export default function SeriesTitleLink({
  titleSlug,
  title,
  ...linkProps
}: SeriesTitleLinkProps) {
  const link = `/series/${titleSlug}`;

  return (
    <Link to={link} {...linkProps}>
      {title}
    </Link>
  );
}
