var Marionette = require('marionette');
var Backbone = require('backbone');
var Backgrid = require('backgrid');
var MissingLayout = require('./Missing/MissingLayout');
var CutoffUnmetLayout = require('./Cutoff/CutoffUnmetLayout');

module.exports = Marionette.Layout.extend({
    template         : 'Wanted/WantedLayoutTemplate',
    regions          : {content : '#content'},
    ui               : {
        missingTab : '.x-missing-tab',
        cutoffTab  : '.x-cutoff-tab'
    },
    events           : {
        "click .x-missing-tab" : '_showMissing',
        "click .x-cutoff-tab"  : '_showCutoffUnmet'
    },
    initialize       : function(options){
        if(options.action) {
            this.action = options.action.toLowerCase();
        }
    },
    onShow           : function(){
        switch (this.action) {
            case 'cutoff':
                this._showCutoffUnmet();
                break;
            default:
                this._showMissing();
        }
    },
    _navigate        : function(route){
        Backbone.history.navigate(route, {
            trigger : false,
            replace : true
        });
    },
    _showMissing     : function(e){
        if(e) {
            e.preventDefault();
        }
        this.content.show(new MissingLayout());
        this.ui.missingTab.tab('show');
        this._navigate('/wanted/missing');
    },
    _showCutoffUnmet : function(e){
        if(e) {
            e.preventDefault();
        }
        this.content.show(new CutoffUnmetLayout());
        this.ui.cutoffTab.tab('show');
        this._navigate('/wanted/cutoff');
    }
});