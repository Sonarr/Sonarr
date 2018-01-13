import PropTypes from 'prop-types';

function createRouteMatchShape(props) {
  return PropTypes.shape({
    params: PropTypes.shape({
      ...props
    }).isRequired
  });
}

export default createRouteMatchShape;
