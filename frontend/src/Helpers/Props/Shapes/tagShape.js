import PropTypes from 'prop-types';

const tagShape = {
  id: PropTypes.oneOfType([PropTypes.bool, PropTypes.number, PropTypes.string]).isRequired,
  name: PropTypes.oneOfType([PropTypes.number, PropTypes.string]).isRequired
};

export default tagShape;
