var $ = require('jquery');
var _ = require('underscore');
var vent = require('../../../vent');
var AppLayout = require('../../../AppLayout');
var Marionette = require('marionette');
var Backbone = require('backbone');
var DelayProfileCollectionView = require('./DelayProfileCollectionView');
var EditView = require('./Edit/DelayProfileEditView');
var Model = require('./DelayProfileModel');

module.exports = Marionette.Layout.extend({
    template                 : 'Settings/Profile/Delay/DelayProfileLayoutTemplate',
    regions                  : {delayProfiles : '.x-rows'},
    events                   : {"click .x-add" : '_add'},
    initialize               : function(options){
        this.collection = options.collection;
        this._updateOrderedCollection();
        this.listenTo(this.collection, 'sync', this._updateOrderedCollection);
        this.listenTo(this.collection, 'add', this._updateOrderedCollection);
        this.listenTo(this.collection, 'remove', function(){
            this.collection.fetch();
        });
    },
    onRender                 : function(){
        this.sortableListView = new DelayProfileCollectionView({
            sortable             : true,
            collection           : this.orderedCollection,
            sortableOptions      : {handle : '.x-drag-handle'},
            sortableModelsFilter : function(model){
                return model.get('id') !== 1;
            }
        });
        this.delayProfiles.show(this.sortableListView);
        this.listenTo(this.sortableListView, 'sortStop', this._updateOrder);
    },
    _updateOrder             : function(){
        var self = this;
        this.collection.forEach(function(model){
            if(model.get('id') === 1) {
                return;
            }
            var orderedModel = self.orderedCollection.get(model);
            var order = self.orderedCollection.indexOf(orderedModel) + 1;
            if(model.get('order') !== order) {
                model.set('order', order);
                model.save();
            }
        });
    },
    _add                     : function(){
        var model = new Model({
            enableUsenet      : true,
            enableTorrent     : true,
            preferredProtocol : 'usenet',
            usenetDelay       : 0,
            torrentDelay      : 0,
            order             : this.collection.length,
            tags              : []
        });
        model.collection = this.collection;
        var view = new EditView({
            model            : model,
            targetCollection : this.collection
        });
        AppLayout.modalRegion.show(view);
    },
    _updateOrderedCollection : function(){
        if(!this.orderedCollection) {
            this.orderedCollection = new Backbone.Collection();
        }
        this.orderedCollection.reset(_.sortBy(this.collection.models, function(model){
            return model.get('order');
        }));
    }
});