import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';
import { kinds } from 'Helpers/Props';
import formatBytes from 'Utilities/Number/formatBytes';

function getTooltip(title, quality, size) {
  if (!title) {
    return;
  }

  const revision = quality.revision;

  if (revision.real && revision.real > 0) {
    title += ' [REAL]';
  }

  if (revision.version && revision.version > 1) {
    title += ' [PROPER]';
  }

  if (size) {
    title += ` - ${formatBytes(size)}`;
  }

  return title;
}

function EpisodeQuality(props) {
  const {
    className,
    title,
    quality,
    size,
    isCutoffNotMet
  } = props;

  if (!quality) {
    return null;
  }

  return (
    <Label
      className={className}
      kind={isCutoffNotMet ? kinds.INVERSE : kinds.DEFAULT}
      title={getTooltip(title, quality, size)}
    >
      {quality.quality.name}
    </Label>
  );
}

EpisodeQuality.propTypes = {
  className: PropTypes.string,
  title: PropTypes.string,
  quality: PropTypes.object.isRequired,
  size: PropTypes.number,
  isCutoffNotMet: PropTypes.bool
};

EpisodeQuality.defaultProps = {
  title: ''
};

export default EpisodeQuality;
