import { EnhancedSelectInputValue } from 'Components/Form/Select/EnhancedSelectInput';
import translate from 'Utilities/String/translate';

export const timeZoneOptions: EnhancedSelectInputValue<string>[] = [
  {
    key: '',
    get value() {
      return translate('SystemDefault');
    },
  },

  // UTC
  { key: 'UTC', value: 'UTC' },

  // Africa (Major cities and unique timezones)
  { key: 'Africa/Abidjan', value: 'Africa/Abidjan' },
  { key: 'Africa/Algiers', value: 'Africa/Algiers' },
  { key: 'Africa/Cairo', value: 'Africa/Cairo' },
  { key: 'Africa/Casablanca', value: 'Africa/Casablanca' },
  { key: 'Africa/Johannesburg', value: 'Africa/Johannesburg' },
  { key: 'Africa/Lagos', value: 'Africa/Lagos' },
  { key: 'Africa/Nairobi', value: 'Africa/Nairobi' },
  { key: 'Africa/Tripoli', value: 'Africa/Tripoli' },

  // America - North America (Major US/Canada zones)
  { key: 'America/New_York', value: 'America/New_York (Eastern)' },
  { key: 'America/Chicago', value: 'America/Chicago (Central)' },
  { key: 'America/Denver', value: 'America/Denver (Mountain)' },
  { key: 'America/Los_Angeles', value: 'America/Los_Angeles (Pacific)' },
  { key: 'America/Anchorage', value: 'America/Anchorage (Alaska)' },
  { key: 'America/Adak', value: 'America/Adak (Hawaii-Aleutian)' },
  { key: 'America/Phoenix', value: 'America/Phoenix (Arizona)' },
  { key: 'America/Toronto', value: 'America/Toronto' },
  { key: 'America/Vancouver', value: 'America/Vancouver' },
  { key: 'America/Halifax', value: 'America/Halifax' },
  { key: 'America/St_Johns', value: 'America/St_Johns (Newfoundland)' },

  // America - Mexico
  { key: 'America/Mexico_City', value: 'America/Mexico_City' },
  { key: 'America/Cancun', value: 'America/Cancun' },
  { key: 'America/Tijuana', value: 'America/Tijuana' },

  // America - Central America
  { key: 'America/Guatemala', value: 'America/Guatemala' },
  { key: 'America/Costa_Rica', value: 'America/Costa_Rica' },
  { key: 'America/Panama', value: 'America/Panama' },

  // America - Caribbean
  { key: 'America/Havana', value: 'America/Havana' },
  { key: 'America/Jamaica', value: 'America/Jamaica' },
  { key: 'America/Puerto_Rico', value: 'America/Puerto_Rico' },

  // America - South America
  { key: 'America/Bogota', value: 'America/Bogota' },
  { key: 'America/Caracas', value: 'America/Caracas' },
  { key: 'America/Guyana', value: 'America/Guyana' },
  { key: 'America/La_Paz', value: 'America/La_Paz' },
  { key: 'America/Lima', value: 'America/Lima' },
  { key: 'America/Santiago', value: 'America/Santiago' },
  { key: 'America/Asuncion', value: 'America/Asuncion' },
  { key: 'America/Montevideo', value: 'America/Montevideo' },
  {
    key: 'America/Argentina/Buenos_Aires',
    value: 'America/Argentina/Buenos_Aires',
  },
  { key: 'America/Sao_Paulo', value: 'America/Sao_Paulo' },
  { key: 'America/Manaus', value: 'America/Manaus' },
  { key: 'America/Fortaleza', value: 'America/Fortaleza' },
  { key: 'America/Noronha', value: 'America/Noronha' },

  // Antarctica (Research stations)
  { key: 'Antarctica/McMurdo', value: 'Antarctica/McMurdo' },
  { key: 'Antarctica/Palmer', value: 'Antarctica/Palmer' },

  // Arctic
  { key: 'Arctic/Longyearbyen', value: 'Arctic/Longyearbyen' },

  // Asia - East Asia
  { key: 'Asia/Tokyo', value: 'Asia/Tokyo' },
  { key: 'Asia/Seoul', value: 'Asia/Seoul' },
  { key: 'Asia/Shanghai', value: 'Asia/Shanghai' },
  { key: 'Asia/Hong_Kong', value: 'Asia/Hong_Kong' },
  { key: 'Asia/Taipei', value: 'Asia/Taipei' },
  { key: 'Asia/Macau', value: 'Asia/Macau' },

  // Asia - Southeast Asia
  { key: 'Asia/Singapore', value: 'Asia/Singapore' },
  { key: 'Asia/Kuala_Lumpur', value: 'Asia/Kuala_Lumpur' },
  { key: 'Asia/Jakarta', value: 'Asia/Jakarta' },
  { key: 'Asia/Manila', value: 'Asia/Manila' },
  { key: 'Asia/Bangkok', value: 'Asia/Bangkok' },
  { key: 'Asia/Ho_Chi_Minh', value: 'Asia/Ho_Chi_Minh' },

  // Asia - South Asia
  { key: 'Asia/Kolkata', value: 'Asia/Kolkata' },
  { key: 'Asia/Dhaka', value: 'Asia/Dhaka' },
  { key: 'Asia/Karachi', value: 'Asia/Karachi' },
  { key: 'Asia/Kathmandu', value: 'Asia/Kathmandu' },
  { key: 'Asia/Colombo', value: 'Asia/Colombo' },

  // Asia - Central Asia
  { key: 'Asia/Almaty', value: 'Asia/Almaty' },
  { key: 'Asia/Tashkent', value: 'Asia/Tashkent' },
  { key: 'Asia/Bishkek', value: 'Asia/Bishkek' },
  { key: 'Asia/Dushanbe', value: 'Asia/Dushanbe' },

  // Asia - Western Asia/Middle East
  { key: 'Asia/Dubai', value: 'Asia/Dubai' },
  { key: 'Asia/Riyadh', value: 'Asia/Riyadh' },
  { key: 'Asia/Kuwait', value: 'Asia/Kuwait' },
  { key: 'Asia/Qatar', value: 'Asia/Qatar' },
  { key: 'Asia/Bahrain', value: 'Asia/Bahrain' },
  { key: 'Asia/Jerusalem', value: 'Asia/Jerusalem' },
  { key: 'Asia/Beirut', value: 'Asia/Beirut' },
  { key: 'Asia/Damascus', value: 'Asia/Damascus' },
  { key: 'Asia/Baghdad', value: 'Asia/Baghdad' },
  { key: 'Asia/Tehran', value: 'Asia/Tehran' },

  // Asia - Russia
  { key: 'Europe/Moscow', value: 'Europe/Moscow' },
  { key: 'Asia/Yekaterinburg', value: 'Asia/Yekaterinburg' },
  { key: 'Asia/Novosibirsk', value: 'Asia/Novosibirsk' },
  { key: 'Asia/Krasnoyarsk', value: 'Asia/Krasnoyarsk' },
  { key: 'Asia/Irkutsk', value: 'Asia/Irkutsk' },
  { key: 'Asia/Yakutsk', value: 'Asia/Yakutsk' },
  { key: 'Asia/Vladivostok', value: 'Asia/Vladivostok' },
  { key: 'Asia/Sakhalin', value: 'Asia/Sakhalin' },
  { key: 'Asia/Kamchatka', value: 'Asia/Kamchatka' },

  // Atlantic
  { key: 'Atlantic/Azores', value: 'Atlantic/Azores' },
  { key: 'Atlantic/Canary', value: 'Atlantic/Canary' },
  { key: 'Atlantic/Cape_Verde', value: 'Atlantic/Cape_Verde' },
  { key: 'Atlantic/Reykjavik', value: 'Atlantic/Reykjavik' },

  // Australia & New Zealand
  { key: 'Australia/Sydney', value: 'Australia/Sydney' },
  { key: 'Australia/Melbourne', value: 'Australia/Melbourne' },
  { key: 'Australia/Brisbane', value: 'Australia/Brisbane' },
  { key: 'Australia/Perth', value: 'Australia/Perth' },
  { key: 'Australia/Adelaide', value: 'Australia/Adelaide' },
  { key: 'Australia/Darwin', value: 'Australia/Darwin' },
  { key: 'Australia/Hobart', value: 'Australia/Hobart' },
  { key: 'Pacific/Auckland', value: 'Pacific/Auckland' },
  { key: 'Pacific/Chatham', value: 'Pacific/Chatham' },

  // Europe - Western Europe
  { key: 'Europe/London', value: 'Europe/London' },
  { key: 'Europe/Dublin', value: 'Europe/Dublin' },
  { key: 'Europe/Paris', value: 'Europe/Paris' },
  { key: 'Europe/Berlin', value: 'Europe/Berlin' },
  { key: 'Europe/Amsterdam', value: 'Europe/Amsterdam' },
  { key: 'Europe/Brussels', value: 'Europe/Brussels' },
  { key: 'Europe/Zurich', value: 'Europe/Zurich' },
  { key: 'Europe/Vienna', value: 'Europe/Vienna' },
  { key: 'Europe/Rome', value: 'Europe/Rome' },
  { key: 'Europe/Madrid', value: 'Europe/Madrid' },
  { key: 'Europe/Lisbon', value: 'Europe/Lisbon' },

  // Europe - Northern Europe
  { key: 'Europe/Stockholm', value: 'Europe/Stockholm' },
  { key: 'Europe/Oslo', value: 'Europe/Oslo' },
  { key: 'Europe/Copenhagen', value: 'Europe/Copenhagen' },
  { key: 'Europe/Helsinki', value: 'Europe/Helsinki' },

  // Europe - Eastern Europe
  { key: 'Europe/Warsaw', value: 'Europe/Warsaw' },
  { key: 'Europe/Prague', value: 'Europe/Prague' },
  { key: 'Europe/Budapest', value: 'Europe/Budapest' },
  { key: 'Europe/Bucharest', value: 'Europe/Bucharest' },
  { key: 'Europe/Sofia', value: 'Europe/Sofia' },
  { key: 'Europe/Athens', value: 'Europe/Athens' },
  { key: 'Europe/Istanbul', value: 'Europe/Istanbul' },
  { key: 'Europe/Kiev', value: 'Europe/Kiev' },
  { key: 'Europe/Minsk', value: 'Europe/Minsk' },

  // Indian Ocean
  { key: 'Indian/Mauritius', value: 'Indian/Mauritius' },
  { key: 'Indian/Maldives', value: 'Indian/Maldives' },

  // Pacific - Major Island Nations
  { key: 'Pacific/Honolulu', value: 'Pacific/Honolulu' },
  { key: 'Pacific/Fiji', value: 'Pacific/Fiji' },
  { key: 'Pacific/Guam', value: 'Pacific/Guam' },
  { key: 'Pacific/Tahiti', value: 'Pacific/Tahiti' },
  { key: 'Pacific/Apia', value: 'Pacific/Apia' },
  { key: 'Pacific/Tongatapu', value: 'Pacific/Tongatapu' },
  { key: 'Pacific/Port_Moresby', value: 'Pacific/Port_Moresby' },
  { key: 'Pacific/Noumea', value: 'Pacific/Noumea' },
];

export default timeZoneOptions;
