var Handlebars = require('handlebars');

Handlebars.registerHelper('formMessage', function(message) {
    if (!message) {
        return '';
    }

    var level = message.type;

    if (message.type === 'error') {
        level = 'danger';
    }

    var messageHtml = '<div class="alert alert-{0}" role="alert">{1}</div>'.format(level, message.message);

    return new Handlebars.SafeString(messageHtml);
});