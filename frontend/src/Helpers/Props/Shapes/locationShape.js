import PropTypes from 'prop-types';

const locationShape = PropTypes.shape({
  pathname: PropTypes.string.isRequired,
  search: PropTypes.string.isRequired,
  state: PropTypes.object,
  action: PropTypes.string,
  key: PropTypes.string
});

export default locationShape;
