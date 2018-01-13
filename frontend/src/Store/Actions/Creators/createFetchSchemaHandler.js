import $ from 'jquery';
import { set } from '../baseActions';

function createFetchSchemaHandler(section, url) {
  return function(getState, payload, dispatch) {
    dispatch(set({ section, isSchemaFetching: true }));

    const promise = $.ajax({
      url
    });

    promise.done((data) => {
      dispatch(set({
        section,
        isSchemaFetching: false,
        isSchemaPopulated: true,
        schemaError: null,
        schema: data
      }));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section,
        isSchemaFetching: false,
        isSchemaPopulated: true,
        schemaError: xhr
      }));
    });
  };
}

export default createFetchSchemaHandler;
