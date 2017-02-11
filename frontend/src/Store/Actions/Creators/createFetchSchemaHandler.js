import $ from 'jquery';
import { set } from '../baseActions';

function createFetchSchemaHandler(section, url) {
  return function(payload) {
    return function(dispatch, getState) {
      dispatch(set({ section, isFetchingSchema: true }));

      const promise = $.ajax({
        url
      });

      promise.done((data) => {
        dispatch(set({
          section,
          isFetchingSchema: false,
          schemaPopulated: true,
          schemaError: null,
          schema: data
        }));
      });

      promise.fail((xhr) => {
        dispatch(set({
          section,
          isFetchingSchema: false,
          schemaPopulated: true,
          schemaError: xhr
        }));
      });
    };
  };
}

export default createFetchSchemaHandler;
