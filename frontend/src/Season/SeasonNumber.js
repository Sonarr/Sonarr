import PropTypes from 'prop-types';

function SeasonNumber(props) {
  const {
    seasonNumber,
    separator
  } = props;

  if (seasonNumber === 0) {
    return `${separator}Specials`;
  }

  if (seasonNumber > 0) {
    return `${separator}Season ${seasonNumber}`;
  }

  return null;
}

SeasonNumber.propTypes = {
  seasonNumber: PropTypes.number.isRequired,
  separator: PropTypes.string.isRequired
};

SeasonNumber.defaultProps = {
  separator: '- '
};

export default SeasonNumber;
