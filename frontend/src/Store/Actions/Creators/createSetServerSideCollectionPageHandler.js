import pages from 'Utilities/pages';

function createSetServerSideCollectionPageHandler(section, page, getFromState, fetchHandler) {
  return function(payload) {
    return function(dispatch, getState) {
      const state = getFromState(getState());
      const sectionState = state.hasOwnProperty(section) ? state[section] : state;
      const currentPage = sectionState.page || 1;
      let nextPage = 0;

      switch (page) {
        case pages.FIRST:
          nextPage = 1;
          break;
        case pages.PREVIOUS:
          nextPage = currentPage - 1;
          break;
        case pages.NEXT:
          nextPage = currentPage + 1;
          break;
        case pages.LAST:
          nextPage = sectionState.totalPages;
          break;
        default:
          nextPage = payload.page;
      }

      // If we prefer to update the page immediately we should
      // set the page and not pass a page to the fetch handler.

      // dispatch(set({ section, page: nextPage }));
      dispatch(fetchHandler({ page: nextPage }));
    };
  };
}

export default createSetServerSideCollectionPageHandler;
