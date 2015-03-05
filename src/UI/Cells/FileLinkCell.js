var vent = require('vent');
var Backgrid = require('backgrid');

module.exports = Backgrid.Cell.extend({
    className : 'file-link-cell',

    render : function() {
        function setCustomReplace() {
            while (true) {
                localStorage.fileLinkReplace = window.prompt("Set characters that shall be replaced:", "/mnt/");
                localStorage.fileLinkReplaceWith = window.prompt("Set what aforeselected characters shall be replaced with", "smb://192.168.1.1/main/");

                var link = makeLink();
                if (window.confirm("Link will be: " + link + ". Click OK if this is correct or you give up. To later modify, clear localStorage.")) {
                    return link;
                }
            }
        }

        function makeLink() {
            if (localStorage.hasOwnProperty("fileLinkReplace") && localStorage.hasOwnProperty("fileLinkReplaceWith")) {
                return path.replace(localStorage.fileLinkReplace, localStorage.fileLinkReplaceWith);
            } else {
                if (window.confirm("Pattern for defining link to " + path + " has not been created. Do it now?")) {
                    return setCustomReplace();
                }
            }
        }

        path = this.model.get('path');
        var link = makeLink();
        if (link) {
            this.$el.empty();
            this.$el.html('<a><i class="icon-open-file x-open-file" title="Open"></a>');
            this.$("a").prop("href", link);
        }

        return this;
    },
});
