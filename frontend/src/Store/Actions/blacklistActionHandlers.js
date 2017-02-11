import serverSideCollectionHandlers from 'Utilities/serverSideCollectionHandlers';
import * as types from './actionTypes';
import createServerSideCollectionHandlers from './Creators/createServerSideCollectionHandlers';

const blacklistActionHandlers = {
  ...createServerSideCollectionHandlers('blacklist', '/blacklist', (state) => state, {
    [serverSideCollectionHandlers.FETCH]: types.FETCH_BLACKLIST,
    [serverSideCollectionHandlers.FIRST_PAGE]: types.GOTO_FIRST_BLACKLIST_PAGE,
    [serverSideCollectionHandlers.PREVIOUS_PAGE]: types.GOTO_PREVIOUS_BLACKLIST_PAGE,
    [serverSideCollectionHandlers.NEXT_PAGE]: types.GOTO_NEXT_BLACKLIST_PAGE,
    [serverSideCollectionHandlers.LAST_PAGE]: types.GOTO_LAST_BLACKLIST_PAGE,
    [serverSideCollectionHandlers.EXACT_PAGE]: types.GOTO_BLACKLIST_PAGE,
    [serverSideCollectionHandlers.SORT]: types.SET_BLACKLIST_SORT
  })
};

export default blacklistActionHandlers;
