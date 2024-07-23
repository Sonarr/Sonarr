import translate from 'Utilities/String/translate';

function formatAge(
  age: string | number,
  ageHours: string | number,
  ageMinutes: string | number
) {
  const ageRounded = Math.round(Number(age));
  const ageHoursFloat = parseFloat(String(ageHours));
  const ageMinutesFloat = ageMinutes && parseFloat(String(ageMinutes));

  if (ageRounded < 2 && ageHoursFloat) {
    if (ageHoursFloat < 2 && !!ageMinutesFloat) {
      return `${ageMinutesFloat.toFixed(0)} ${
        ageHoursFloat === 1
          ? translate('FormatAgeMinute')
          : translate('FormatAgeMinutes')
      }`;
    }

    return `${ageHoursFloat.toFixed(1)} ${
      ageHoursFloat === 1
        ? translate('FormatAgeHour')
        : translate('FormatAgeHours')
    }`;
  }

  return `${ageRounded} ${
    ageRounded === 1 ? translate('FormatAgeDay') : translate('FormatAgeDays')
  }`;
}

export default formatAge;
