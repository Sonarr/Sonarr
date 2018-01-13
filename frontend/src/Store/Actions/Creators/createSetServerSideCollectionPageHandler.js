import pages from 'Utilities/pages';
import getSectionState from 'Utilities/State/getSectionState';

function createSetServerSideCollectionPageHandler(section, page, fetchHandler) {
  return function(getState, payload, dispatch) {
    const sectionState = getSectionState(getState(), section, true);
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
}

export default createSetServerSideCollectionPageHandler;
