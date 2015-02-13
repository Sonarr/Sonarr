var $ = require('jquery');
var StatusModel = require('../System/StatusModel');
var ZeroClipboard = require('zero.clipboard');
var Messenger = require('../Shared/Messenger');

$.fn.copyToClipboard = function(input) {

    ZeroClipboard.config({
        swfPath : StatusModel.get('urlBase') + '/Content/zero.clipboard.swf'
    });

    var client = new ZeroClipboard(this);

    client.on('ready', function(e) {
        client.on('copy', function(e) {
            e.clipboardData.setData("text/plain", input.val());
        });
        client.on('aftercopy', function() {
            Messenger.show({ message : 'Copied text to clipboard' });
        });
    });
};