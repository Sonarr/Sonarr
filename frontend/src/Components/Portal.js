import PropTypes from 'prop-types';
import ReactDOM from 'react-dom';

function Portal(props) {
  const { children, target } = props;
  return ReactDOM.createPortal(children, target);
}

Portal.propTypes = {
  children: PropTypes.node.isRequired,
  target: PropTypes.object.isRequired
};

Portal.defaultProps = {
  target: document.getElementById('portal-root')
};

export default Portal;
