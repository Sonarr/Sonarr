var Marionette = require('marionette');

module.exports = Marionette.ItemView.extend({
    template : 'Settings/Profile/Edit/EditProfileItemViewTemplate',
    ui: {
        cutoff: ".x-cutoff"
    },

    onRender: function() {
        if (this.model.get("quality").id === this.collectionListView.model.get("cutoff").id) {
            this.ui.cutoff.prop("checked", true);
        }
    },

    events : {
        'click .cutoff': '_onRadioSelect'
    },

    _onRadioSelect: function(e) {
        this.collectionListView.model.set("cutoff", this.model.get("quality"));

        // Do not let event bubble, as the parent will register the click as select / deselect
        e.stopPropagation();
    }
});
