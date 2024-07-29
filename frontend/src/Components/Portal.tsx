import ReactDOM from 'react-dom';

interface PortalProps {
  children: Parameters<typeof ReactDOM.createPortal>[0];
  target?: Parameters<typeof ReactDOM.createPortal>[1];
}

const defaultTarget = document.getElementById('portal-root');

function Portal(props: PortalProps) {
  const { children, target = defaultTarget } = props;

  if (!target) {
    return null;
  }

  return ReactDOM.createPortal(children, target);
}

export default Portal;
