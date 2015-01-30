'use strict';

define(
    [
        'marionette',
        'Settings/Quality/Definition/QualityDefinitionItemView'
    ], function (Marionette, QualityDefinitionItemView) {
    
        return Marionette.CompositeView.extend({
            template: 'Settings/Quality/Definition/QualityDefinitionCollectionTemplate',
            
            itemViewContainer: '.x-rows',
            
            itemView: QualityDefinitionItemView
        });
    });
