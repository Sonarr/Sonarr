import formatUnit from 'Utilities/Number/formatUnit';

function formatAge(
  age: string | number,
  ageHours: string | number,
  ageMinutes: string | number
) {
  const ageRounded = Math.round(Number(age));
  const ageHoursFloat = parseFloat(String(ageHours));
  const ageMinutesFloat = parseFloat(String(ageMinutes));

  if (ageRounded < 2 && ageHoursFloat) {
    if (ageHoursFloat < 2 && ageMinutesFloat) {
      return formatUnit(ageMinutesFloat, 'minute');
    }

    return formatUnit(ageHoursFloat, 'hour', { maximumFractionDigits: 1 });
  }

  return formatUnit(ageRounded, 'day');
}

export default formatAge;
