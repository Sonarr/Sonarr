var vent = require('../../vent');
var QualityCell = require('../../Cells/QualityCell');
var SelectQualityLayout = require('../Quality/SelectQualityLayout');

module.exports = QualityCell.extend({
    className : 'quality-cell editable',

    events : {
        'click' : '_onClick'
    },

    _onClick : function () {
        var view =  new SelectQualityLayout();

        this.listenTo(view, 'manualimport:selected:quality', this._setQuality);

        vent.trigger(vent.Commands.OpenModal2Command, view);
    },

    _setQuality : function (e) {
        this.model.set('quality', e.quality);
    }
});