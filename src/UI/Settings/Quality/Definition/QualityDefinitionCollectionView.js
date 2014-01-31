'use strict';

define(
    [
        'marionette',
        'backgrid',
        'Settings/Quality/Definition/QualityDefinitionView'
    ], function (Marionette, Backgrid, QualityDefinitionView) {
    
        return Marionette.CompositeView.extend({
            template: 'Settings/Quality/Definition/QualityDefinitionCollectionTemplate',
            
            itemViewContainer: ".x-rows",
            
            itemView: QualityDefinitionView
        });
    });
