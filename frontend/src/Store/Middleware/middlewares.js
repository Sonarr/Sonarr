import { applyMiddleware, compose } from 'redux';
import thunk from 'redux-thunk';
import { routerMiddleware } from 'react-router-redux';
import sentryMiddleware from './sentryMiddleware';
import persistState from './persistState';

export default function(history) {
  const middlewares = [];
  const ravenMiddleware = sentryMiddleware();

  if (ravenMiddleware) {
    middlewares.push(ravenMiddleware);
  }

  middlewares.push(routerMiddleware(history));
  middlewares.push(thunk);

  return compose(
    applyMiddleware(...middlewares),
    persistState
  );
}
