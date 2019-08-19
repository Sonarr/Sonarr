import { createThunk, handleThunks } from 'Store/thunks';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import { set } from './baseActions';

//
// Variables

export const section = 'diagnostic';
const scriptSection = 'diagnostic.script';

//
// State

const exampleScript = `// Obtain the instance of ISeriesService
var seriesService = Resolve<ISeriesService>();

// Get all series
var series = seriesService.GetAllSeries();

// Find the top 5 highest rated ones
var top5 = series.Where(s => s.Ratings.Votes > 6)
                 .OrderByDescending(s => s.Ratings.Value)
                 .Take(5)
                 .Select(s => s.Title);

return new {
  Top5 = top5,
  Count = series.Count()
};`;

export const defaultState = {
  status: {
    isFetching: false,
    isPopulated: false,
    error: null,
    item: {}
  },

  script: {
    isExecuting: false,
    isDebugging: false,
    isValidating: false,
    workspaceId: null,
    code: exampleScript,
    validation: null,
    result: null,
    error: null
  }
};

//
// Actions Types

export const FETCH_STATUS = 'diagnostic/status/fetchStatus';
export const UPDATE_SCRIPT = 'diagnostic/script/update';
export const VALIDATE_SCRIPT = 'diagnostic/script/validate';
export const EXECUTE_SCRIPT = 'diagnostic/script/execute';

//
// Action Creators

export const fetchStatus = createThunk(FETCH_STATUS);
export const updateScript = createThunk(UPDATE_SCRIPT);
export const validateScript = createThunk(VALIDATE_SCRIPT);
export const executeScript = createThunk(EXECUTE_SCRIPT);

//
// Action Handlers

export const actionHandlers = handleThunks({
  [FETCH_STATUS]: createFetchHandler('diagnostic.status', '/diagnostic/status'),

  [UPDATE_SCRIPT]: function(getState, payload, dispatch) {
    const {
      code
    } = payload;

    dispatch(set({
      section: scriptSection,
      code
    }));
  },

  [VALIDATE_SCRIPT]: function(getState, payload, dispatch) {
    const {
      code
    } = payload;

    dispatch(set({
      section: scriptSection,
      code,
      isValidating: true
    }));

    let ajaxOptions = null;

    ajaxOptions = {
      url: '/diagnostic/script/validate',
      method: 'POST',
      contentType: 'application/json',
      dataType: 'json',
      data: JSON.stringify({
        code
      })
    };

    const promise = createAjaxRequest(ajaxOptions).request;

    promise.done((data) => {
      dispatch(set({
        section: scriptSection,
        isValidating: false,
        validation: data
      }));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section: scriptSection,
        isValidating: false,
        validation: null,
        error: xhr
      }));
    });
  },

  [EXECUTE_SCRIPT]: function(getState, payload, dispatch) {
    const {
      code,
      debug
    } = payload;

    dispatch(set({
      section: scriptSection,
      code,
      isExecuting: !debug,
      isDebugging: debug
    }));

    let ajaxOptions = null;

    ajaxOptions = {
      url: '/diagnostic/script/execute',
      method: 'POST',
      contentType: 'application/json',
      dataType: 'json',
      data: JSON.stringify({
        code,
        debug
      })
    };

    const promise = createAjaxRequest(ajaxOptions).request;

    promise.done((data) => {
      dispatch(set({
        section: scriptSection,
        isExecuting: false,
        isDebugging: false,
        result: (debug || data.error) ? data : data.returnValue,
        validation: {
          errorDiagnostics: data.errorDiagnostics
        }
      }));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section: scriptSection,
        isExecuting: false,
        isDebugging: false,
        error: xhr
      }));
    });
  }
});

//
// Reducers

export const reducers = createHandleActions({

}, defaultState, section);
