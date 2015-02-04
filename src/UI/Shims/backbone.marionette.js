require('backbone');
require('../Handlebars/backbone.marionette.templates');
require('../Mixins/AsNamedView');
require('../JsLibraries/backbone.marionette');

var templateMixin = require('../Handlebars/backbone.marionette.templates');
var asNamedView = require('../Mixins/AsNamedView');

templateMixin.call(window.Marionette.TemplateCache);


asNamedView.call(window.Marionette.ItemView.prototype);


module.exports = window.Marionette;