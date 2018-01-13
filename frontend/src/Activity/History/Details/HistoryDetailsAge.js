var Handlebars = require('handlebars');
var FormatHelpers = require('Shared/FormatHelpers');

Handlebars.registerHelper('historyAge', function() {
  var age = this.age;
  var unit = FormatHelpers.plural(Math.round(age), 'day');
  var ageHours = parseFloat(this.ageHours);
  var ageMinutes = this.ageMinutes ? parseFloat(this.ageMinutes) : null;

  if (age < 2) {
    age = ageHours.toFixed(1);
    unit = FormatHelpers.plural(Math.round(ageHours), 'hour');
  }

  if (age < 2 && ageMinutes) {
    age = parseFloat(ageMinutes).toFixed(1);
    unit = FormatHelpers.plural(Math.round(ageMinutes), 'minute');
  }

  return new Handlebars.SafeString(`<dt>Age (when grabbed):</dt><dd>${age} ${unit}</dd>`);
});
