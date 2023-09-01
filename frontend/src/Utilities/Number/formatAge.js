import translate from 'Utilities/String/translate';

function formatAge(age, ageHours, ageMinutes) {
  age = Math.round(age);
  ageHours = parseFloat(ageHours);
  ageMinutes = ageMinutes && parseFloat(ageMinutes);

  if (age < 2 && ageHours) {
    if (ageHours < 2 && !!ageMinutes) {
      return `${ageMinutes.toFixed(0)} ${ageHours === 1 ? translate('Minute').toLowerCase() : translate('Minutes').toLowerCase()}`;
    }

    return `${ageHours.toFixed(1)} ${ageHours === 1 ? translate('Hour').toLowerCase() : translate('Hours').toLowerCase()}`;
  }

  return `${age} ${age === 1 ? translate('Day').toLowerCase() : translate('Days').toLowerCase()}`;
}

export default formatAge;
