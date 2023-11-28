import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';
import { kinds } from 'Helpers/Props';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';

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

function revisionLabel(className, quality, showRevision) {
  if (!showRevision) {
    return;
  }

  if (quality.revision.isRepack) {
    return (
      <Label
        className={className}
        kind={kinds.PRIMARY}
        title={translate('Repack')}
      >
        R
      </Label>
    );
  }

  if (quality.revision.version && quality.revision.version > 1) {
    return (
      <Label
        className={className}
        kind={kinds.PRIMARY}
        title={translate('Proper')}
      >
        P
      </Label>
    );
  }
}

function EpisodeQuality(props) {
  const {
    className,
    title,
    quality,
    size,
    isCutoffNotMet,
    showRevision
  } = props;

  if (!quality) {
    return null;
  }

  return (
    <span>
      <Label
        className={className}
        kind={isCutoffNotMet ? kinds.INVERSE : kinds.DEFAULT}
        title={getTooltip(title, quality, size)}
      >
        {quality.quality.name}
      </Label>{revisionLabel(className, quality, showRevision)}
    </span>
  );
}

EpisodeQuality.propTypes = {
  className: PropTypes.string,
  title: PropTypes.string,
  quality: PropTypes.object.isRequired,
  size: PropTypes.number,
  isCutoffNotMet: PropTypes.bool,
  showRevision: PropTypes.bool
};

EpisodeQuality.defaultProps = {
  title: '',
  showRevision: false
};

export default EpisodeQuality;
