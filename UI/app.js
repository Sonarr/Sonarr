'use strict';
require.config({

    urlArgs: 'v=' + window.ServerStatus.version,

    paths: {
        'backbone'            : 'JsLibraries/backbone',
        'sugar'               : 'JsLibraries/sugar',
        'handlebars'          : 'JsLibraries/handlebars.runtime',
        'handlebars.helpers'  : 'JsLibraries/handlebars.helpers',
        'bootstrap'           : 'JsLibraries/bootstrap',
        'backbone.mutators'   : 'JsLibraries/backbone.mutators',
        'backbone.deepmodel'  : 'JsLibraries/backbone.deep.model',
        'backbone.pageable'   : 'JsLibraries/backbone.pageable',
        'backbone.modelbinder': 'JsLibraries/backbone.modelbinder',
        'backgrid'            : 'JsLibraries/backbone.backgrid',
        'backgrid.paginator'  : 'JsLibraries/backbone.backgrid.paginator',
        'fullcalendar'        : 'JsLibraries/fullcalendar',
        'backstrech'          : 'JsLibraries/jquery.backstretch',
        '$'                   : 'JsLibraries/jquery',
        'underscore'          : 'JsLibraries/lodash.underscore',
        'marionette'          : 'JsLibraries/backbone.marionette',
        'signalR'             : 'JsLibraries/jquery.signalR',
        'jquery.knob'         : 'JsLibraries/jquery.knob',
        'libs'                : 'JsLibraries/'

    },

    shim: {

        $: {
            deps   :
                [
                    'Instrumentation/ErrorHandler'
                ],
            exports: '$',

            init: function () {
                require(
                    [
                        'jQuery/ToTheTop'
                    ]);
            }

        },

        signalR: {
            deps:
                [
                    '$'
                ]
        },

        bootstrap: {
            deps:
                [
                    '$'
                ]
        },

        backstrech: {
            deps:
                [
                    '$'
                ]
        },

        underscore: {
            deps   :
                [
                    '$'
                ],
            exports: '_'
        },

        backbone: {
            deps   :
                [
                    'mixins/backbone.ajax',
                    'underscore',
                    '$'
                ],
            exports: 'Backbone',
            init   : function (AjaxMixin) {
                AjaxMixin.apply(Backbone);
            }
        },


        'backbone.deepmodel': {
            deps:
                [
                    'mixins/underscore.mixin.deepExtend'
                ]
        },

        marionette: {
            deps:
                [
                    'backbone',
                    'Handlebars/backbone.marionette.templates',
                    'mixins/AsNamedView'
                ],

            exports: 'Marionette',
            init   : function (Backbone, TemplateMixin, AsNamedView) {
                TemplateMixin.call(Marionette.TemplateCache);
                AsNamedView.call(Marionette.ItemView.prototype);

            }
        },

        'jquery.knob': {
            deps:
                [
                    '$'
                ]
        },

        'backbone.pageable': {
            deps:
                [
                    'backbone'
                ]
        },

        backgrid            : {
            deps:
                [
                    'backbone'
                ],

            exports: 'Backgrid',

            init: function () {
                require(
                    [
                        'Shared/Grid/HeaderCell'
                    ], function () {

                        Backgrid.Column.prototype.defaults = {
                            name      : undefined,
                            label     : undefined,
                            sortable  : true,
                            editable  : false,
                            renderable: true,
                            formatter : undefined,
                            cell      : undefined,
                            headerCell: 'NzbDrone'
                        };

                    });
            }
        },
        'backgrid.paginator': {

            exports: 'Backgrid.Extension.Paginator',

            deps:
                [
                    'backgrid'
                ]
        },

        'handlebars.helpers': {
            deps:
                [
                    'handlebars'
                ]
        }
    }
});

define(
    [
        'marionette',
        'shared/modal/region',
        'Instrumentation/StringFormat',
    ], function (Marionette, ModalRegion) {

        require(
            [
                'libs/backbone.mutators'
            ]);


        var app = new Marionette.Application();

        app.Events = {
            SeriesAdded: 'seriesAdded'
        };

        app.Commands = {
            SaveSettings: 'saveSettings'
        };


        app.addInitializer(function () {
            console.log('starting application');
        });

        app.addRegions({
            mainRegion        : '#main-region',
            modalRegion       : ModalRegion,
            footerRegion      : '#footer-region'
        });

        app.start();

        window.require(
            [
                'Router',
                'jQuery/TooltipBinder'
            ]);

        return app;
    });






