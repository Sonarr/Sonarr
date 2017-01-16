require('backbone');
require('../JsLibraries/backbone.validation');
var $ = require('jquery');

var jqueryValidation = require('../jQuery/jquery.validation');
jqueryValidation.call($);

module.exports = window.Backbone.Validation;