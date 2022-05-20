import pages from 'Utilities/pages';
import serverSideCollectionHandlers from 'Utilities/serverSideCollectionHandlers';
import createFetchServerSideCollectionHandler from './createFetchServerSideCollectionHandler';
import createSetServerSideCollectionFilterHandler from './createSetServerSideCollectionFilterHandler';
import createSetServerSideCollectionPageHandler from './createSetServerSideCollectionPageHandler';
import createSetServerSideCollectionSortHandler from './createSetServerSideCollectionSortHandler';

function createServerSideCollectionHandlers(section, url, fetchThunk, handlers, fetchDataAugmenter) {
  const actionHandlers = {};
  const fetchHandlerType = handlers[serverSideCollectionHandlers.FETCH];
  const fetchHandler = createFetchServerSideCollectionHandler(section, url, fetchDataAugmenter);
  actionHandlers[fetchHandlerType] = fetchHandler;

  if (handlers.hasOwnProperty(serverSideCollectionHandlers.FIRST_PAGE)) {
    const handlerType = handlers[serverSideCollectionHandlers.FIRST_PAGE];
    actionHandlers[handlerType] = createSetServerSideCollectionPageHandler(section, pages.FIRST, fetchThunk);
  }

  if (handlers.hasOwnProperty(serverSideCollectionHandlers.PREVIOUS_PAGE)) {
    const handlerType = handlers[serverSideCollectionHandlers.PREVIOUS_PAGE];
    actionHandlers[handlerType] = createSetServerSideCollectionPageHandler(section, pages.PREVIOUS, fetchThunk);
  }

  if (handlers.hasOwnProperty(serverSideCollectionHandlers.NEXT_PAGE)) {
    const handlerType = handlers[serverSideCollectionHandlers.NEXT_PAGE];
    actionHandlers[handlerType] = createSetServerSideCollectionPageHandler(section, pages.NEXT, fetchThunk);
  }

  if (handlers.hasOwnProperty(serverSideCollectionHandlers.LAST_PAGE)) {
    const handlerType = handlers[serverSideCollectionHandlers.LAST_PAGE];
    actionHandlers[handlerType] = createSetServerSideCollectionPageHandler(section, pages.LAST, fetchThunk);
  }

  if (handlers.hasOwnProperty(serverSideCollectionHandlers.EXACT_PAGE)) {
    const handlerType = handlers[serverSideCollectionHandlers.EXACT_PAGE];
    actionHandlers[handlerType] = createSetServerSideCollectionPageHandler(section, pages.EXACT, fetchThunk);
  }

  if (handlers.hasOwnProperty(serverSideCollectionHandlers.SORT)) {
    const handlerType = handlers[serverSideCollectionHandlers.SORT];
    actionHandlers[handlerType] = createSetServerSideCollectionSortHandler(section, fetchThunk);
  }

  if (handlers.hasOwnProperty(serverSideCollectionHandlers.FILTER)) {
    const handlerType = handlers[serverSideCollectionHandlers.FILTER];
    actionHandlers[handlerType] = createSetServerSideCollectionFilterHandler(section, fetchThunk);
  }

  return actionHandlers;
}

export default createServerSideCollectionHandlers;
