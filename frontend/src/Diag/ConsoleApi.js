import createAjaxRequest from 'Utilities/createAjaxRequest';

// This file contains some helpers for power users in a browser console

let hasWarned = false;

function checkActivationWarning() {
  if (!hasWarned) {
    console.log('Activated SonarrApi console helpers.');
    console.warn('Be warned: There will be no further confirmation checks.');
    hasWarned = true;
  }
}

function attachAsyncActions(promise) {
  promise.filter = function() {
    const args = arguments;
    const res = this.then((d) => d.filter(...args));
    attachAsyncActions(res);
    return res;
  };

  promise.map = function() {
    const args = arguments;
    const res = this.then((d) => d.map(...args));
    attachAsyncActions(res);
    return res;
  };

  promise.all = function() {
    const res = this.then((d) => Promise.all(d));
    attachAsyncActions(res);
    return res;
  };

  promise.forEach = function(action) {
    const res = this.then((d) => Promise.all(d.map(action)));
    attachAsyncActions(res);
    return res;
  };
}

class ResourceApi {
  constructor(api, url) {
    this.api = api;
    this.url = url;
  }

  single(id) {
    return this.api.fetch(`${this.url}/${id}`);
  }

  all() {
    return this.api.fetch(this.url);
  }

  filter(pred) {
    return this.all().filter(pred);
  }

  update(resource) {
    return this.api.fetch(`${this.url}/${resource.id}`, { method: 'PUT', data: resource });
  }

  delete(resource) {
    if (typeof resource === 'object' && resource !== null && resource.id) {
      resource = resource.id;
    }

    if (!resource || !Number.isInteger(resource)) {
      throw Error('Invalid resource', resource);
    }

    return this.api.fetch(`${this.url}/${resource}`, { method: 'DELETE' });
  }

  fetch(url, options) {
    return this.api.fetch(`${this.url}${url}`, options);
  }
}

class ConsoleApi {
  constructor() {
    this.series = new ResourceApi(this, '/series');
  }

  resource(url) {
    return new ResourceApi(this, url);
  }

  fetch(url, options) {
    checkActivationWarning();

    options = options || {};

    const req = {
      url,
      method: options.method || 'GET'
    };

    if (options.data) {
      req.dataType = 'json';
      req.data = JSON.stringify(options.data);
    }

    const promise = createAjaxRequest(req).request;

    promise.fail((xhr) => {
      console.error(`Failed to fetch ${url}`, xhr);
    });

    attachAsyncActions(promise);

    return promise;
  }
}

window.SonarrApi = new ConsoleApi();

export default ConsoleApi;
