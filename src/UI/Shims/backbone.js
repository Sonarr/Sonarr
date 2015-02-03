require('jquery');
require('../JsLibraries/backbone');

var jquery = require('jquery');
var backbone = require('../JsLibraries/backbone');
backbone.$ = jquery;

window.Backbone = backbone;
module.exports = backbone;