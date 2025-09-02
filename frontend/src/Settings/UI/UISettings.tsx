import React, { useCallback, useEffect, useMemo } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import { EnhancedSelectInputValue } from 'Components/Form/Select/EnhancedSelectInput';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import { inputTypes, kinds } from 'Helpers/Props';
import SettingsToolbar from 'Settings/SettingsToolbar';
import {
  fetchUISettings,
  saveUISettings,
  setUISettingsValue,
} from 'Store/Actions/settingsActions';
import createLanguagesSelector from 'Store/Selectors/createLanguagesSelector';
import createSettingsSectionSelector from 'Store/Selectors/createSettingsSectionSelector';
import themes from 'Styles/Themes';
import { InputChanged } from 'typings/inputs';
import titleCase from 'Utilities/String/titleCase';
import translate from 'Utilities/String/translate';

const SECTION = 'ui';

export const firstDayOfWeekOptions: EnhancedSelectInputValue<number>[] = [
  {
    key: 0,
    get value() {
      return translate('Sunday');
    },
  },
  {
    key: 1,
    get value() {
      return translate('Monday');
    },
  },
];

export const weekColumnOptions: EnhancedSelectInputValue<string>[] = [
  { key: 'ddd M/D', value: 'Tue 3/25', hint: 'ddd M/D' },
  { key: 'ddd MM/DD', value: 'Tue 03/25', hint: 'ddd MM/DD' },
  { key: 'ddd D/M', value: 'Tue 25/3', hint: 'ddd D/M' },
  { key: 'ddd DD/MM', value: 'Tue 25/03', hint: 'ddd DD/MM' },
];

const shortDateFormatOptions: EnhancedSelectInputValue<string>[] = [
  { key: 'MMM D YYYY', value: 'Mar 25 2014', hint: 'MMM D YYYY' },
  { key: 'DD MMM YYYY', value: '25 Mar 2014', hint: 'DD MMM YYYY' },
  { key: 'MM/D/YYYY', value: '03/25/2014', hint: 'MM/D/YYYY' },
  { key: 'MM/DD/YYYY', value: '03/25/2014', hint: 'MM/DD/YYYY' },
  { key: 'DD/MM/YYYY', value: '25/03/2014', hint: 'DD/MM/YYYY' },
  { key: 'YYYY-MM-DD', value: '2014-03-25', hint: 'YYYY-MM-DD' },
];

const longDateFormatOptions: EnhancedSelectInputValue<string>[] = [
  { key: 'dddd, MMMM D YYYY', value: 'Tuesday, March 25, 2014' },
  { key: 'dddd, D MMMM YYYY', value: 'Tuesday, 25 March, 2014' },
];

export const timeFormatOptions: EnhancedSelectInputValue<string>[] = [
  { key: 'h(:mm)a', value: '5pm/5:30pm' },
  { key: 'HH:mm', value: '17:00/17:30' },
];

const timeZoneOptions: EnhancedSelectInputValue<string>[] = [
  { key: '', value: 'System Default' },
  
  // UTC
  { key: 'UTC', value: 'UTC' },
  
  // Africa
  { key: 'Africa/Abidjan', value: 'Africa/Abidjan (Ivory Coast)' },
  { key: 'Africa/Accra', value: 'Africa/Accra (Ghana)' },
  { key: 'Africa/Addis_Ababa', value: 'Africa/Addis_Ababa (Ethiopia)' },
  { key: 'Africa/Algiers', value: 'Africa/Algiers (Algeria)' },
  { key: 'Africa/Asmara', value: 'Africa/Asmara (Eritrea)' },
  { key: 'Africa/Bamako', value: 'Africa/Bamako (Mali)' },
  { key: 'Africa/Bangui', value: 'Africa/Bangui (Central African Republic)' },
  { key: 'Africa/Banjul', value: 'Africa/Banjul (Gambia)' },
  { key: 'Africa/Bissau', value: 'Africa/Bissau (Guinea-Bissau)' },
  { key: 'Africa/Blantyre', value: 'Africa/Blantyre (Malawi)' },
  { key: 'Africa/Brazzaville', value: 'Africa/Brazzaville (Republic of the Congo)' },
  { key: 'Africa/Bujumbura', value: 'Africa/Bujumbura (Burundi)' },
  { key: 'Africa/Cairo', value: 'Africa/Cairo (Egypt)' },
  { key: 'Africa/Casablanca', value: 'Africa/Casablanca (Morocco)' },
  { key: 'Africa/Ceuta', value: 'Africa/Ceuta (Spain)' },
  { key: 'Africa/Conakry', value: 'Africa/Conakry (Guinea)' },
  { key: 'Africa/Dakar', value: 'Africa/Dakar (Senegal)' },
  { key: 'Africa/Dar_es_Salaam', value: 'Africa/Dar_es_Salaam (Tanzania)' },
  { key: 'Africa/Djibouti', value: 'Africa/Djibouti (Djibouti)' },
  { key: 'Africa/Douala', value: 'Africa/Douala (Cameroon)' },
  { key: 'Africa/El_Aaiun', value: 'Africa/El_Aaiun (Western Sahara)' },
  { key: 'Africa/Freetown', value: 'Africa/Freetown (Sierra Leone)' },
  { key: 'Africa/Gaborone', value: 'Africa/Gaborone (Botswana)' },
  { key: 'Africa/Harare', value: 'Africa/Harare (Zimbabwe)' },
  { key: 'Africa/Johannesburg', value: 'Africa/Johannesburg (South Africa)' },
  { key: 'Africa/Juba', value: 'Africa/Juba (South Sudan)' },
  { key: 'Africa/Kampala', value: 'Africa/Kampala (Uganda)' },
  { key: 'Africa/Khartoum', value: 'Africa/Khartoum (Sudan)' },
  { key: 'Africa/Kigali', value: 'Africa/Kigali (Rwanda)' },
  { key: 'Africa/Kinshasa', value: 'Africa/Kinshasa (Democratic Republic of Congo)' },
  { key: 'Africa/Lagos', value: 'Africa/Lagos (Nigeria)' },
  { key: 'Africa/Libreville', value: 'Africa/Libreville (Gabon)' },
  { key: 'Africa/Lome', value: 'Africa/Lome (Togo)' },
  { key: 'Africa/Luanda', value: 'Africa/Luanda (Angola)' },
  { key: 'Africa/Lubumbashi', value: 'Africa/Lubumbashi (Democratic Republic of Congo)' },
  { key: 'Africa/Lusaka', value: 'Africa/Lusaka (Zambia)' },
  { key: 'Africa/Malabo', value: 'Africa/Malabo (Equatorial Guinea)' },
  { key: 'Africa/Maputo', value: 'Africa/Maputo (Mozambique)' },
  { key: 'Africa/Maseru', value: 'Africa/Maseru (Lesotho)' },
  { key: 'Africa/Mbabane', value: 'Africa/Mbabane (Eswatini)' },
  { key: 'Africa/Mogadishu', value: 'Africa/Mogadishu (Somalia)' },
  { key: 'Africa/Monrovia', value: 'Africa/Monrovia (Liberia)' },
  { key: 'Africa/Nairobi', value: 'Africa/Nairobi (Kenya)' },
  { key: 'Africa/Ndjamena', value: 'Africa/Ndjamena (Chad)' },
  { key: 'Africa/Niamey', value: 'Africa/Niamey (Niger)' },
  { key: 'Africa/Nouakchott', value: 'Africa/Nouakchott (Mauritania)' },
  { key: 'Africa/Ouagadougou', value: 'Africa/Ouagadougou (Burkina Faso)' },
  { key: 'Africa/Porto-Novo', value: 'Africa/Porto-Novo (Benin)' },
  { key: 'Africa/Sao_Tome', value: 'Africa/Sao_Tome (São Tomé and Príncipe)' },
  { key: 'Africa/Tripoli', value: 'Africa/Tripoli (Libya)' },
  { key: 'Africa/Tunis', value: 'Africa/Tunis (Tunisia)' },
  { key: 'Africa/Windhoek', value: 'Africa/Windhoek (Namibia)' },
  
  // America - North
  { key: 'America/Adak', value: 'America/Adak (Alaska)' },
  { key: 'America/Anchorage', value: 'America/Anchorage (Alaska)' },
  { key: 'America/Anguilla', value: 'America/Anguilla (Anguilla)' },
  { key: 'America/Antigua', value: 'America/Antigua (Antigua and Barbuda)' },
  { key: 'America/Araguaina', value: 'America/Araguaina (Brazil)' },
  { key: 'America/Argentina/Buenos_Aires', value: 'America/Argentina/Buenos_Aires (Argentina)' },
  { key: 'America/Argentina/Catamarca', value: 'America/Argentina/Catamarca (Argentina)' },
  { key: 'America/Argentina/Cordoba', value: 'America/Argentina/Cordoba (Argentina)' },
  { key: 'America/Argentina/Jujuy', value: 'America/Argentina/Jujuy (Argentina)' },
  { key: 'America/Argentina/La_Rioja', value: 'America/Argentina/La_Rioja (Argentina)' },
  { key: 'America/Argentina/Mendoza', value: 'America/Argentina/Mendoza (Argentina)' },
  { key: 'America/Argentina/Rio_Gallegos', value: 'America/Argentina/Rio_Gallegos (Argentina)' },
  { key: 'America/Argentina/Salta', value: 'America/Argentina/Salta (Argentina)' },
  { key: 'America/Argentina/San_Juan', value: 'America/Argentina/San_Juan (Argentina)' },
  { key: 'America/Argentina/San_Luis', value: 'America/Argentina/San_Luis (Argentina)' },
  { key: 'America/Argentina/Tucuman', value: 'America/Argentina/Tucuman (Argentina)' },
  { key: 'America/Argentina/Ushuaia', value: 'America/Argentina/Ushuaia (Argentina)' },
  { key: 'America/Aruba', value: 'America/Aruba (Aruba)' },
  { key: 'America/Asuncion', value: 'America/Asuncion (Paraguay)' },
  { key: 'America/Atikokan', value: 'America/Atikokan (Canada)' },
  { key: 'America/Bahia', value: 'America/Bahia (Brazil)' },
  { key: 'America/Bahia_Banderas', value: 'America/Bahia_Banderas (Mexico)' },
  { key: 'America/Barbados', value: 'America/Barbados (Barbados)' },
  { key: 'America/Belem', value: 'America/Belem (Brazil)' },
  { key: 'America/Belize', value: 'America/Belize (Belize)' },
  { key: 'America/Blanc-Sablon', value: 'America/Blanc-Sablon (Canada)' },
  { key: 'America/Boa_Vista', value: 'America/Boa_Vista (Brazil)' },
  { key: 'America/Bogota', value: 'America/Bogota (Colombia)' },
  { key: 'America/Boise', value: 'America/Boise (Idaho)' },
  { key: 'America/Cambridge_Bay', value: 'America/Cambridge_Bay (Canada)' },
  { key: 'America/Campo_Grande', value: 'America/Campo_Grande (Brazil)' },
  { key: 'America/Cancun', value: 'America/Cancun (Mexico)' },
  { key: 'America/Caracas', value: 'America/Caracas (Venezuela)' },
  { key: 'America/Cayenne', value: 'America/Cayenne (French Guiana)' },
  { key: 'America/Cayman', value: 'America/Cayman (Cayman Islands)' },
  { key: 'America/Chicago', value: 'America/Chicago (Central Time)' },
  { key: 'America/Chihuahua', value: 'America/Chihuahua (Mexico)' },
  { key: 'America/Costa_Rica', value: 'America/Costa_Rica (Costa Rica)' },
  { key: 'America/Creston', value: 'America/Creston (Canada)' },
  { key: 'America/Cuiaba', value: 'America/Cuiaba (Brazil)' },
  { key: 'America/Curacao', value: 'America/Curacao (Curaçao)' },
  { key: 'America/Danmarkshavn', value: 'America/Danmarkshavn (Greenland)' },
  { key: 'America/Dawson', value: 'America/Dawson (Canada)' },
  { key: 'America/Dawson_Creek', value: 'America/Dawson_Creek (Canada)' },
  { key: 'America/Denver', value: 'America/Denver (Mountain Time)' },
  { key: 'America/Detroit', value: 'America/Detroit (Michigan)' },
  { key: 'America/Dominica', value: 'America/Dominica (Dominica)' },
  { key: 'America/Edmonton', value: 'America/Edmonton (Canada)' },
  { key: 'America/Eirunepe', value: 'America/Eirunepe (Brazil)' },
  { key: 'America/El_Salvador', value: 'America/El_Salvador (El Salvador)' },
  { key: 'America/Fort_Nelson', value: 'America/Fort_Nelson (Canada)' },
  { key: 'America/Fortaleza', value: 'America/Fortaleza (Brazil)' },
  { key: 'America/Glace_Bay', value: 'America/Glace_Bay (Canada)' },
  { key: 'America/Godthab', value: 'America/Godthab (Greenland)' },
  { key: 'America/Goose_Bay', value: 'America/Goose_Bay (Canada)' },
  { key: 'America/Grand_Turk', value: 'America/Grand_Turk (Turks and Caicos)' },
  { key: 'America/Grenada', value: 'America/Grenada (Grenada)' },
  { key: 'America/Guadeloupe', value: 'America/Guadeloupe (Guadeloupe)' },
  { key: 'America/Guatemala', value: 'America/Guatemala (Guatemala)' },
  { key: 'America/Guayaquil', value: 'America/Guayaquil (Ecuador)' },
  { key: 'America/Guyana', value: 'America/Guyana (Guyana)' },
  { key: 'America/Halifax', value: 'America/Halifax (Canada)' },
  { key: 'America/Havana', value: 'America/Havana (Cuba)' },
  { key: 'America/Hermosillo', value: 'America/Hermosillo (Mexico)' },
  { key: 'America/Indiana/Indianapolis', value: 'America/Indiana/Indianapolis (Indiana)' },
  { key: 'America/Indiana/Knox', value: 'America/Indiana/Knox (Indiana)' },
  { key: 'America/Indiana/Marengo', value: 'America/Indiana/Marengo (Indiana)' },
  { key: 'America/Indiana/Petersburg', value: 'America/Indiana/Petersburg (Indiana)' },
  { key: 'America/Indiana/Tell_City', value: 'America/Indiana/Tell_City (Indiana)' },
  { key: 'America/Indiana/Vevay', value: 'America/Indiana/Vevay (Indiana)' },
  { key: 'America/Indiana/Vincennes', value: 'America/Indiana/Vincennes (Indiana)' },
  { key: 'America/Indiana/Winamac', value: 'America/Indiana/Winamac (Indiana)' },
  { key: 'America/Inuvik', value: 'America/Inuvik (Canada)' },
  { key: 'America/Iqaluit', value: 'America/Iqaluit (Canada)' },
  { key: 'America/Jamaica', value: 'America/Jamaica (Jamaica)' },
  { key: 'America/Juneau', value: 'America/Juneau (Alaska)' },
  { key: 'America/Kentucky/Louisville', value: 'America/Kentucky/Louisville (Kentucky)' },
  { key: 'America/Kentucky/Monticello', value: 'America/Kentucky/Monticello (Kentucky)' },
  { key: 'America/Kralendijk', value: 'America/Kralendijk (Caribbean Netherlands)' },
  { key: 'America/La_Paz', value: 'America/La_Paz (Bolivia)' },
  { key: 'America/Lima', value: 'America/Lima (Peru)' },
  { key: 'America/Los_Angeles', value: 'America/Los_Angeles (Pacific Time)' },
  { key: 'America/Lower_Princes', value: 'America/Lower_Princes (Sint Maarten)' },
  { key: 'America/Maceio', value: 'America/Maceio (Brazil)' },
  { key: 'America/Managua', value: 'America/Managua (Nicaragua)' },
  { key: 'America/Manaus', value: 'America/Manaus (Brazil)' },
  { key: 'America/Marigot', value: 'America/Marigot (Saint Martin)' },
  { key: 'America/Martinique', value: 'America/Martinique (Martinique)' },
  { key: 'America/Matamoros', value: 'America/Matamoros (Mexico)' },
  { key: 'America/Mazatlan', value: 'America/Mazatlan (Mexico)' },
  { key: 'America/Menominee', value: 'America/Menominee (Michigan)' },
  { key: 'America/Merida', value: 'America/Merida (Mexico)' },
  { key: 'America/Metlakatla', value: 'America/Metlakatla (Alaska)' },
  { key: 'America/Mexico_City', value: 'America/Mexico_City (Mexico)' },
  { key: 'America/Miquelon', value: 'America/Miquelon (Saint Pierre and Miquelon)' },
  { key: 'America/Moncton', value: 'America/Moncton (Canada)' },
  { key: 'America/Monterrey', value: 'America/Monterrey (Mexico)' },
  { key: 'America/Montevideo', value: 'America/Montevideo (Uruguay)' },
  { key: 'America/Montserrat', value: 'America/Montserrat (Montserrat)' },
  { key: 'America/Nassau', value: 'America/Nassau (Bahamas)' },
  { key: 'America/New_York', value: 'America/New_York (Eastern Time)' },
  { key: 'America/Nipigon', value: 'America/Nipigon (Canada)' },
  { key: 'America/Nome', value: 'America/Nome (Alaska)' },
  { key: 'America/Noronha', value: 'America/Noronha (Brazil)' },
  { key: 'America/North_Dakota/Beulah', value: 'America/North_Dakota/Beulah (North Dakota)' },
  { key: 'America/North_Dakota/Center', value: 'America/North_Dakota/Center (North Dakota)' },
  { key: 'America/North_Dakota/New_Salem', value: 'America/North_Dakota/New_Salem (North Dakota)' },
  { key: 'America/Ojinaga', value: 'America/Ojinaga (Mexico)' },
  { key: 'America/Panama', value: 'America/Panama (Panama)' },
  { key: 'America/Pangnirtung', value: 'America/Pangnirtung (Canada)' },
  { key: 'America/Paramaribo', value: 'America/Paramaribo (Suriname)' },
  { key: 'America/Phoenix', value: 'America/Phoenix (Arizona)' },
  { key: 'America/Port-au-Prince', value: 'America/Port-au-Prince (Haiti)' },
  { key: 'America/Port_of_Spain', value: 'America/Port_of_Spain (Trinidad and Tobago)' },
  { key: 'America/Porto_Velho', value: 'America/Porto_Velho (Brazil)' },
  { key: 'America/Puerto_Rico', value: 'America/Puerto_Rico (Puerto Rico)' },
  { key: 'America/Punta_Arenas', value: 'America/Punta_Arenas (Chile)' },
  { key: 'America/Rainy_River', value: 'America/Rainy_River (Canada)' },
  { key: 'America/Rankin_Inlet', value: 'America/Rankin_Inlet (Canada)' },
  { key: 'America/Recife', value: 'America/Recife (Brazil)' },
  { key: 'America/Regina', value: 'America/Regina (Canada)' },
  { key: 'America/Resolute', value: 'America/Resolute (Canada)' },
  { key: 'America/Rio_Branco', value: 'America/Rio_Branco (Brazil)' },
  { key: 'America/Santarem', value: 'America/Santarem (Brazil)' },
  { key: 'America/Santiago', value: 'America/Santiago (Chile)' },
  { key: 'America/Santo_Domingo', value: 'America/Santo_Domingo (Dominican Republic)' },
  { key: 'America/Sao_Paulo', value: 'America/Sao_Paulo (Brazil)' },
  { key: 'America/Scoresbysund', value: 'America/Scoresbysund (Greenland)' },
  { key: 'America/Sitka', value: 'America/Sitka (Alaska)' },
  { key: 'America/St_Barthelemy', value: 'America/St_Barthelemy (Saint Barthélemy)' },
  { key: 'America/St_Johns', value: 'America/St_Johns (Canada)' },
  { key: 'America/St_Kitts', value: 'America/St_Kitts (Saint Kitts and Nevis)' },
  { key: 'America/St_Lucia', value: 'America/St_Lucia (Saint Lucia)' },
  { key: 'America/St_Thomas', value: 'America/St_Thomas (US Virgin Islands)' },
  { key: 'America/St_Vincent', value: 'America/St_Vincent (Saint Vincent and the Grenadines)' },
  { key: 'America/Swift_Current', value: 'America/Swift_Current (Canada)' },
  { key: 'America/Tegucigalpa', value: 'America/Tegucigalpa (Honduras)' },
  { key: 'America/Thule', value: 'America/Thule (Greenland)' },
  { key: 'America/Thunder_Bay', value: 'America/Thunder_Bay (Canada)' },
  { key: 'America/Tijuana', value: 'America/Tijuana (Mexico)' },
  { key: 'America/Toronto', value: 'America/Toronto (Canada)' },
  { key: 'America/Tortola', value: 'America/Tortola (British Virgin Islands)' },
  { key: 'America/Vancouver', value: 'America/Vancouver (Canada)' },
  { key: 'America/Whitehorse', value: 'America/Whitehorse (Canada)' },
  { key: 'America/Winnipeg', value: 'America/Winnipeg (Canada)' },
  { key: 'America/Yakutat', value: 'America/Yakutat (Alaska)' },
  { key: 'America/Yellowknife', value: 'America/Yellowknife (Canada)' },
  
  // Antarctica
  { key: 'Antarctica/Casey', value: 'Antarctica/Casey (Antarctica)' },
  { key: 'Antarctica/Davis', value: 'Antarctica/Davis (Antarctica)' },
  { key: 'Antarctica/DumontDUrville', value: 'Antarctica/DumontDUrville (Antarctica)' },
  { key: 'Antarctica/Macquarie', value: 'Antarctica/Macquarie (Antarctica)' },
  { key: 'Antarctica/Mawson', value: 'Antarctica/Mawson (Antarctica)' },
  { key: 'Antarctica/McMurdo', value: 'Antarctica/McMurdo (Antarctica)' },
  { key: 'Antarctica/Palmer', value: 'Antarctica/Palmer (Antarctica)' },
  { key: 'Antarctica/Rothera', value: 'Antarctica/Rothera (Antarctica)' },
  { key: 'Antarctica/Syowa', value: 'Antarctica/Syowa (Antarctica)' },
  { key: 'Antarctica/Troll', value: 'Antarctica/Troll (Antarctica)' },
  { key: 'Antarctica/Vostok', value: 'Antarctica/Vostok (Antarctica)' },
  
  // Arctic
  { key: 'Arctic/Longyearbyen', value: 'Arctic/Longyearbyen (Svalbard)' },
  
  // Asia
  { key: 'Asia/Aden', value: 'Asia/Aden (Yemen)' },
  { key: 'Asia/Almaty', value: 'Asia/Almaty (Kazakhstan)' },
  { key: 'Asia/Amman', value: 'Asia/Amman (Jordan)' },
  { key: 'Asia/Anadyr', value: 'Asia/Anadyr (Russia)' },
  { key: 'Asia/Aqtau', value: 'Asia/Aqtau (Kazakhstan)' },
  { key: 'Asia/Aqtobe', value: 'Asia/Aqtobe (Kazakhstan)' },
  { key: 'Asia/Ashgabat', value: 'Asia/Ashgabat (Turkmenistan)' },
  { key: 'Asia/Atyrau', value: 'Asia/Atyrau (Kazakhstan)' },
  { key: 'Asia/Baghdad', value: 'Asia/Baghdad (Iraq)' },
  { key: 'Asia/Bahrain', value: 'Asia/Bahrain (Bahrain)' },
  { key: 'Asia/Baku', value: 'Asia/Baku (Azerbaijan)' },
  { key: 'Asia/Bangkok', value: 'Asia/Bangkok (Thailand)' },
  { key: 'Asia/Barnaul', value: 'Asia/Barnaul (Russia)' },
  { key: 'Asia/Beirut', value: 'Asia/Beirut (Lebanon)' },
  { key: 'Asia/Bishkek', value: 'Asia/Bishkek (Kyrgyzstan)' },
  { key: 'Asia/Brunei', value: 'Asia/Brunei (Brunei)' },
  { key: 'Asia/Chita', value: 'Asia/Chita (Russia)' },
  { key: 'Asia/Choibalsan', value: 'Asia/Choibalsan (Mongolia)' },
  { key: 'Asia/Colombo', value: 'Asia/Colombo (Sri Lanka)' },
  { key: 'Asia/Damascus', value: 'Asia/Damascus (Syria)' },
  { key: 'Asia/Dhaka', value: 'Asia/Dhaka (Bangladesh)' },
  { key: 'Asia/Dili', value: 'Asia/Dili (East Timor)' },
  { key: 'Asia/Dubai', value: 'Asia/Dubai (UAE)' },
  { key: 'Asia/Dushanbe', value: 'Asia/Dushanbe (Tajikistan)' },
  { key: 'Asia/Famagusta', value: 'Asia/Famagusta (Cyprus)' },
  { key: 'Asia/Gaza', value: 'Asia/Gaza (Palestine)' },
  { key: 'Asia/Hebron', value: 'Asia/Hebron (Palestine)' },
  { key: 'Asia/Ho_Chi_Minh', value: 'Asia/Ho_Chi_Minh (Vietnam)' },
  { key: 'Asia/Hong_Kong', value: 'Asia/Hong_Kong (Hong Kong)' },
  { key: 'Asia/Hovd', value: 'Asia/Hovd (Mongolia)' },
  { key: 'Asia/Irkutsk', value: 'Asia/Irkutsk (Russia)' },
  { key: 'Asia/Jakarta', value: 'Asia/Jakarta (Indonesia)' },
  { key: 'Asia/Jayapura', value: 'Asia/Jayapura (Indonesia)' },
  { key: 'Asia/Jerusalem', value: 'Asia/Jerusalem (Israel)' },
  { key: 'Asia/Kabul', value: 'Asia/Kabul (Afghanistan)' },
  { key: 'Asia/Kamchatka', value: 'Asia/Kamchatka (Russia)' },
  { key: 'Asia/Karachi', value: 'Asia/Karachi (Pakistan)' },
  { key: 'Asia/Kathmandu', value: 'Asia/Kathmandu (Nepal)' },
  { key: 'Asia/Khandyga', value: 'Asia/Khandyga (Russia)' },
  { key: 'Asia/Kolkata', value: 'Asia/Kolkata (India)' },
  { key: 'Asia/Krasnoyarsk', value: 'Asia/Krasnoyarsk (Russia)' },
  { key: 'Asia/Kuala_Lumpur', value: 'Asia/Kuala_Lumpur (Malaysia)' },
  { key: 'Asia/Kuching', value: 'Asia/Kuching (Malaysia)' },
  { key: 'Asia/Kuwait', value: 'Asia/Kuwait (Kuwait)' },
  { key: 'Asia/Macau', value: 'Asia/Macau (Macau)' },
  { key: 'Asia/Magadan', value: 'Asia/Magadan (Russia)' },
  { key: 'Asia/Makassar', value: 'Asia/Makassar (Indonesia)' },
  { key: 'Asia/Manila', value: 'Asia/Manila (Philippines)' },
  { key: 'Asia/Muscat', value: 'Asia/Muscat (Oman)' },
  { key: 'Asia/Nicosia', value: 'Asia/Nicosia (Cyprus)' },
  { key: 'Asia/Novokuznetsk', value: 'Asia/Novokuznetsk (Russia)' },
  { key: 'Asia/Novosibirsk', value: 'Asia/Novosibirsk (Russia)' },
  { key: 'Asia/Omsk', value: 'Asia/Omsk (Russia)' },
  { key: 'Asia/Oral', value: 'Asia/Oral (Kazakhstan)' },
  { key: 'Asia/Phnom_Penh', value: 'Asia/Phnom_Penh (Cambodia)' },
  { key: 'Asia/Pontianak', value: 'Asia/Pontianak (Indonesia)' },
  { key: 'Asia/Pyongyang', value: 'Asia/Pyongyang (North Korea)' },
  { key: 'Asia/Qatar', value: 'Asia/Qatar (Qatar)' },
  { key: 'Asia/Qostanay', value: 'Asia/Qostanay (Kazakhstan)' },
  { key: 'Asia/Qyzylorda', value: 'Asia/Qyzylorda (Kazakhstan)' },
  { key: 'Asia/Riyadh', value: 'Asia/Riyadh (Saudi Arabia)' },
  { key: 'Asia/Sakhalin', value: 'Asia/Sakhalin (Russia)' },
  { key: 'Asia/Samarkand', value: 'Asia/Samarkand (Uzbekistan)' },
  { key: 'Asia/Seoul', value: 'Asia/Seoul (South Korea)' },
  { key: 'Asia/Shanghai', value: 'Asia/Shanghai (China)' },
  { key: 'Asia/Singapore', value: 'Asia/Singapore (Singapore)' },
  { key: 'Asia/Srednekolymsk', value: 'Asia/Srednekolymsk (Russia)' },
  { key: 'Asia/Taipei', value: 'Asia/Taipei (Taiwan)' },
  { key: 'Asia/Tashkent', value: 'Asia/Tashkent (Uzbekistan)' },
  { key: 'Asia/Tbilisi', value: 'Asia/Tbilisi (Georgia)' },
  { key: 'Asia/Tehran', value: 'Asia/Tehran (Iran)' },
  { key: 'Asia/Thimphu', value: 'Asia/Thimphu (Bhutan)' },
  { key: 'Asia/Tokyo', value: 'Asia/Tokyo (Japan)' },
  { key: 'Asia/Tomsk', value: 'Asia/Tomsk (Russia)' },
  { key: 'Asia/Ulaanbaatar', value: 'Asia/Ulaanbaatar (Mongolia)' },
  { key: 'Asia/Urumqi', value: 'Asia/Urumqi (China)' },
  { key: 'Asia/Ust-Nera', value: 'Asia/Ust-Nera (Russia)' },
  { key: 'Asia/Vientiane', value: 'Asia/Vientiane (Laos)' },
  { key: 'Asia/Vladivostok', value: 'Asia/Vladivostok (Russia)' },
  { key: 'Asia/Yakutsk', value: 'Asia/Yakutsk (Russia)' },
  { key: 'Asia/Yangon', value: 'Asia/Yangon (Myanmar)' },
  { key: 'Asia/Yekaterinburg', value: 'Asia/Yekaterinburg (Russia)' },
  { key: 'Asia/Yerevan', value: 'Asia/Yerevan (Armenia)' },
  
  // Atlantic
  { key: 'Atlantic/Azores', value: 'Atlantic/Azores (Portugal)' },
  { key: 'Atlantic/Bermuda', value: 'Atlantic/Bermuda (Bermuda)' },
  { key: 'Atlantic/Canary', value: 'Atlantic/Canary (Spain)' },
  { key: 'Atlantic/Cape_Verde', value: 'Atlantic/Cape_Verde (Cape Verde)' },
  { key: 'Atlantic/Faroe', value: 'Atlantic/Faroe (Faroe Islands)' },
  { key: 'Atlantic/Madeira', value: 'Atlantic/Madeira (Portugal)' },
  { key: 'Atlantic/Reykjavik', value: 'Atlantic/Reykjavik (Iceland)' },
  { key: 'Atlantic/South_Georgia', value: 'Atlantic/South_Georgia (South Georgia)' },
  { key: 'Atlantic/St_Helena', value: 'Atlantic/St_Helena (Saint Helena)' },
  { key: 'Atlantic/Stanley', value: 'Atlantic/Stanley (Falkland Islands)' },
  
  // Australia
  { key: 'Australia/Adelaide', value: 'Australia/Adelaide (South Australia)' },
  { key: 'Australia/Brisbane', value: 'Australia/Brisbane (Queensland)' },
  { key: 'Australia/Broken_Hill', value: 'Australia/Broken_Hill (New South Wales)' },
  { key: 'Australia/Currie', value: 'Australia/Currie (Tasmania)' },
  { key: 'Australia/Darwin', value: 'Australia/Darwin (Northern Territory)' },
  { key: 'Australia/Eucla', value: 'Australia/Eucla (Western Australia)' },
  { key: 'Australia/Hobart', value: 'Australia/Hobart (Tasmania)' },
  { key: 'Australia/Lindeman', value: 'Australia/Lindeman (Queensland)' },
  { key: 'Australia/Lord_Howe', value: 'Australia/Lord_Howe (New South Wales)' },
  { key: 'Australia/Melbourne', value: 'Australia/Melbourne (Victoria)' },
  { key: 'Australia/Perth', value: 'Australia/Perth (Western Australia)' },
  { key: 'Australia/Sydney', value: 'Australia/Sydney (New South Wales)' },
  
  // Europe
  { key: 'Europe/Amsterdam', value: 'Europe/Amsterdam (Netherlands)' },
  { key: 'Europe/Andorra', value: 'Europe/Andorra (Andorra)' },
  { key: 'Europe/Astrakhan', value: 'Europe/Astrakhan (Russia)' },
  { key: 'Europe/Athens', value: 'Europe/Athens (Greece)' },
  { key: 'Europe/Belgrade', value: 'Europe/Belgrade (Serbia)' },
  { key: 'Europe/Berlin', value: 'Europe/Berlin (Germany)' },
  { key: 'Europe/Bratislava', value: 'Europe/Bratislava (Slovakia)' },
  { key: 'Europe/Brussels', value: 'Europe/Brussels (Belgium)' },
  { key: 'Europe/Bucharest', value: 'Europe/Bucharest (Romania)' },
  { key: 'Europe/Budapest', value: 'Europe/Budapest (Hungary)' },
  { key: 'Europe/Busingen', value: 'Europe/Busingen (Germany)' },
  { key: 'Europe/Chisinau', value: 'Europe/Chisinau (Moldova)' },
  { key: 'Europe/Copenhagen', value: 'Europe/Copenhagen (Denmark)' },
  { key: 'Europe/Dublin', value: 'Europe/Dublin (Ireland)' },
  { key: 'Europe/Gibraltar', value: 'Europe/Gibraltar (Gibraltar)' },
  { key: 'Europe/Guernsey', value: 'Europe/Guernsey (Guernsey)' },
  { key: 'Europe/Helsinki', value: 'Europe/Helsinki (Finland)' },
  { key: 'Europe/Isle_of_Man', value: 'Europe/Isle_of_Man (Isle of Man)' },
  { key: 'Europe/Istanbul', value: 'Europe/Istanbul (Turkey)' },
  { key: 'Europe/Jersey', value: 'Europe/Jersey (Jersey)' },
  { key: 'Europe/Kaliningrad', value: 'Europe/Kaliningrad (Russia)' },
  { key: 'Europe/Kiev', value: 'Europe/Kiev (Ukraine)' },
  { key: 'Europe/Kirov', value: 'Europe/Kirov (Russia)' },
  { key: 'Europe/Lisbon', value: 'Europe/Lisbon (Portugal)' },
  { key: 'Europe/Ljubljana', value: 'Europe/Ljubljana (Slovenia)' },
  { key: 'Europe/London', value: 'Europe/London (United Kingdom)' },
  { key: 'Europe/Luxembourg', value: 'Europe/Luxembourg (Luxembourg)' },
  { key: 'Europe/Madrid', value: 'Europe/Madrid (Spain)' },
  { key: 'Europe/Malta', value: 'Europe/Malta (Malta)' },
  { key: 'Europe/Mariehamn', value: 'Europe/Mariehamn (Finland)' },
  { key: 'Europe/Minsk', value: 'Europe/Minsk (Belarus)' },
  { key: 'Europe/Monaco', value: 'Europe/Monaco (Monaco)' },
  { key: 'Europe/Moscow', value: 'Europe/Moscow (Russia)' },
  { key: 'Europe/Oslo', value: 'Europe/Oslo (Norway)' },
  { key: 'Europe/Paris', value: 'Europe/Paris (France)' },
  { key: 'Europe/Podgorica', value: 'Europe/Podgorica (Montenegro)' },
  { key: 'Europe/Prague', value: 'Europe/Prague (Czech Republic)' },
  { key: 'Europe/Riga', value: 'Europe/Riga (Latvia)' },
  { key: 'Europe/Rome', value: 'Europe/Rome (Italy)' },
  { key: 'Europe/Samara', value: 'Europe/Samara (Russia)' },
  { key: 'Europe/San_Marino', value: 'Europe/San_Marino (San Marino)' },
  { key: 'Europe/Sarajevo', value: 'Europe/Sarajevo (Bosnia and Herzegovina)' },
  { key: 'Europe/Saratov', value: 'Europe/Saratov (Russia)' },
  { key: 'Europe/Simferopol', value: 'Europe/Simferopol (Ukraine)' },
  { key: 'Europe/Skopje', value: 'Europe/Skopje (North Macedonia)' },
  { key: 'Europe/Sofia', value: 'Europe/Sofia (Bulgaria)' },
  { key: 'Europe/Stockholm', value: 'Europe/Stockholm (Sweden)' },
  { key: 'Europe/Tallinn', value: 'Europe/Tallinn (Estonia)' },
  { key: 'Europe/Tirane', value: 'Europe/Tirane (Albania)' },
  { key: 'Europe/Ulyanovsk', value: 'Europe/Ulyanovsk (Russia)' },
  { key: 'Europe/Uzhgorod', value: 'Europe/Uzhgorod (Ukraine)' },
  { key: 'Europe/Vaduz', value: 'Europe/Vaduz (Liechtenstein)' },
  { key: 'Europe/Vatican', value: 'Europe/Vatican (Vatican City)' },
  { key: 'Europe/Vienna', value: 'Europe/Vienna (Austria)' },
  { key: 'Europe/Vilnius', value: 'Europe/Vilnius (Lithuania)' },
  { key: 'Europe/Volgograd', value: 'Europe/Volgograd (Russia)' },
  { key: 'Europe/Warsaw', value: 'Europe/Warsaw (Poland)' },
  { key: 'Europe/Zagreb', value: 'Europe/Zagreb (Croatia)' },
  { key: 'Europe/Zaporozhye', value: 'Europe/Zaporozhye (Ukraine)' },
  { key: 'Europe/Zurich', value: 'Europe/Zurich (Switzerland)' },
  
  // Indian Ocean
  { key: 'Indian/Antananarivo', value: 'Indian/Antananarivo (Madagascar)' },
  { key: 'Indian/Chagos', value: 'Indian/Chagos (British Indian Ocean Territory)' },
  { key: 'Indian/Christmas', value: 'Indian/Christmas (Christmas Island)' },
  { key: 'Indian/Cocos', value: 'Indian/Cocos (Cocos Islands)' },
  { key: 'Indian/Comoro', value: 'Indian/Comoro (Comoros)' },
  { key: 'Indian/Kerguelen', value: 'Indian/Kerguelen (French Southern Territories)' },
  { key: 'Indian/Mahe', value: 'Indian/Mahe (Seychelles)' },
  { key: 'Indian/Maldives', value: 'Indian/Maldives (Maldives)' },
  { key: 'Indian/Mauritius', value: 'Indian/Mauritius (Mauritius)' },
  { key: 'Indian/Mayotte', value: 'Indian/Mayotte (Mayotte)' },
  { key: 'Indian/Reunion', value: 'Indian/Reunion (Réunion)' },
  
  // Pacific
  { key: 'Pacific/Apia', value: 'Pacific/Apia (Samoa)' },
  { key: 'Pacific/Auckland', value: 'Pacific/Auckland (New Zealand)' },
  { key: 'Pacific/Bougainville', value: 'Pacific/Bougainville (Papua New Guinea)' },
  { key: 'Pacific/Chatham', value: 'Pacific/Chatham (New Zealand)' },
  { key: 'Pacific/Chuuk', value: 'Pacific/Chuuk (Federated States of Micronesia)' },
  { key: 'Pacific/Easter', value: 'Pacific/Easter (Chile)' },
  { key: 'Pacific/Efate', value: 'Pacific/Efate (Vanuatu)' },
  { key: 'Pacific/Enderbury', value: 'Pacific/Enderbury (Kiribati)' },
  { key: 'Pacific/Fakaofo', value: 'Pacific/Fakaofo (Tokelau)' },
  { key: 'Pacific/Fiji', value: 'Pacific/Fiji (Fiji)' },
  { key: 'Pacific/Funafuti', value: 'Pacific/Funafuti (Tuvalu)' },
  { key: 'Pacific/Galapagos', value: 'Pacific/Galapagos (Ecuador)' },
  { key: 'Pacific/Gambier', value: 'Pacific/Gambier (French Polynesia)' },
  { key: 'Pacific/Guadalcanal', value: 'Pacific/Guadalcanal (Solomon Islands)' },
  { key: 'Pacific/Guam', value: 'Pacific/Guam (Guam)' },
  { key: 'Pacific/Honolulu', value: 'Pacific/Honolulu (Hawaii)' },
  { key: 'Pacific/Kiritimati', value: 'Pacific/Kiritimati (Kiribati)' },
  { key: 'Pacific/Kosrae', value: 'Pacific/Kosrae (Federated States of Micronesia)' },
  { key: 'Pacific/Kwajalein', value: 'Pacific/Kwajalein (Marshall Islands)' },
  { key: 'Pacific/Majuro', value: 'Pacific/Majuro (Marshall Islands)' },
  { key: 'Pacific/Marquesas', value: 'Pacific/Marquesas (French Polynesia)' },
  { key: 'Pacific/Midway', value: 'Pacific/Midway (United States Minor Outlying Islands)' },
  { key: 'Pacific/Nauru', value: 'Pacific/Nauru (Nauru)' },
  { key: 'Pacific/Niue', value: 'Pacific/Niue (Niue)' },
  { key: 'Pacific/Norfolk', value: 'Pacific/Norfolk (Norfolk Island)' },
  { key: 'Pacific/Noumea', value: 'Pacific/Noumea (New Caledonia)' },
  { key: 'Pacific/Pago_Pago', value: 'Pacific/Pago_Pago (American Samoa)' },
  { key: 'Pacific/Palau', value: 'Pacific/Palau (Palau)' },
  { key: 'Pacific/Pitcairn', value: 'Pacific/Pitcairn (Pitcairn Islands)' },
  { key: 'Pacific/Pohnpei', value: 'Pacific/Pohnpei (Federated States of Micronesia)' },
  { key: 'Pacific/Port_Moresby', value: 'Pacific/Port_Moresby (Papua New Guinea)' },
  { key: 'Pacific/Rarotonga', value: 'Pacific/Rarotonga (Cook Islands)' },
  { key: 'Pacific/Saipan', value: 'Pacific/Saipan (Northern Mariana Islands)' },
  { key: 'Pacific/Tahiti', value: 'Pacific/Tahiti (French Polynesia)' },
  { key: 'Pacific/Tarawa', value: 'Pacific/Tarawa (Kiribati)' },
  { key: 'Pacific/Tongatapu', value: 'Pacific/Tongatapu (Tonga)' },
  { key: 'Pacific/Wake', value: 'Pacific/Wake (United States Minor Outlying Islands)' },
  { key: 'Pacific/Wallis', value: 'Pacific/Wallis (Wallis and Futuna)' },
];

function UISettings() {
  const dispatch = useDispatch();

  const {
    items,
    isFetching: isLanguagesFetching,
    isPopulated: isLanguagesPopulated,
    error: languagesError,
  } = useSelector(
    createLanguagesSelector({
      Any: true,
      Original: true,
      Unknown: true,
    })
  );

  const {
    isFetching: isSettingsFetching,
    isPopulated: isSettingsPopulated,
    error: settingsError,
    hasSettings,
    settings,
    hasPendingChanges,
    isSaving,
    validationErrors,
    validationWarnings,
  } = useSelector(createSettingsSectionSelector(SECTION));

  const isFetching = isLanguagesFetching || isSettingsFetching;
  const isPopulated = isLanguagesPopulated && isSettingsPopulated;
  const error = languagesError || settingsError;

  const languages = useMemo(() => {
    return items.map((item) => {
      return {
        key: item.id,
        value: item.name,
      };
    });
  }, [items]);

  const themeOptions = Object.keys(themes).map((theme) => ({
    key: theme,
    value: titleCase(theme),
  }));

  const handleInputChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error - actions aren't typed
      dispatch(setUISettingsValue(change));
    },
    [dispatch]
  );
  const handleSavePress = useCallback(() => {
    dispatch(saveUISettings());
  }, [dispatch]);

  useEffect(() => {
    dispatch(fetchUISettings());

    return () => {
      // @ts-expect-error - actions aren't typed
      dispatch(setUISettingsValue({ section: `settings.${SECTION}` }));
    };
  }, [dispatch]);

  return (
    <PageContent title={translate('UiSettings')}>
      <SettingsToolbar
        hasPendingChanges={hasPendingChanges}
        isSaving={isSaving}
        onSavePress={handleSavePress}
      />

      <PageContentBody>
        {isFetching && isPopulated ? <LoadingIndicator /> : null}

        {!isFetching && error ? (
          <Alert kind={kinds.DANGER}>{translate('UiSettingsLoadError')}</Alert>
        ) : null}

        {hasSettings && isPopulated && !error ? (
          <Form
            id="uiSettings"
            validationErrors={validationErrors}
            validationWarnings={validationWarnings}
          >
            <FieldSet legend={translate('Calendar')}>
              <FormGroup>
                <FormLabel>{translate('FirstDayOfWeek')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="firstDayOfWeek"
                  values={firstDayOfWeekOptions}
                  onChange={handleInputChange}
                  {...settings.firstDayOfWeek}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('WeekColumnHeader')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="calendarWeekColumnHeader"
                  values={weekColumnOptions}
                  helpText={translate('WeekColumnHeaderHelpText')}
                  onChange={handleInputChange}
                  {...settings.calendarWeekColumnHeader}
                />
              </FormGroup>
            </FieldSet>

            <FieldSet legend={translate('Dates')}>
              <FormGroup>
                <FormLabel>{translate('ShortDateFormat')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="shortDateFormat"
                  values={shortDateFormatOptions}
                  onChange={handleInputChange}
                  {...settings.shortDateFormat}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('LongDateFormat')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="longDateFormat"
                  values={longDateFormatOptions}
                  onChange={handleInputChange}
                  {...settings.longDateFormat}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('TimeFormat')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="timeFormat"
                  values={timeFormatOptions}
                  onChange={handleInputChange}
                  {...settings.timeFormat}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('TimeZone')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="timeZone"
                  values={timeZoneOptions}
                  onChange={handleInputChange}
                  {...settings.timeZone}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('ShowRelativeDates')}</FormLabel>
                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="showRelativeDates"
                  helpText={translate('ShowRelativeDatesHelpText')}
                  onChange={handleInputChange}
                  {...settings.showRelativeDates}
                />
              </FormGroup>
            </FieldSet>

            <FieldSet legend={translate('Style')}>
              <FormGroup>
                <FormLabel>{translate('Theme')}</FormLabel>
                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="theme"
                  helpText={translate('ThemeHelpText')}
                  values={themeOptions}
                  onChange={handleInputChange}
                  {...settings.theme}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('EnableColorImpairedMode')}</FormLabel>
                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="enableColorImpairedMode"
                  helpText={translate('EnableColorImpairedModeHelpText')}
                  onChange={handleInputChange}
                  {...settings.enableColorImpairedMode}
                />
              </FormGroup>
            </FieldSet>

            <FieldSet legend={translate('Language')}>
              <FormGroup>
                <FormLabel>{translate('UiLanguage')}</FormLabel>
                <FormInputGroup
                  type={inputTypes.LANGUAGE_SELECT}
                  name="uiLanguage"
                  helpText={translate('UiLanguageHelpText')}
                  helpTextWarning={translate('BrowserReloadRequired')}
                  onChange={handleInputChange}
                  {...settings.uiLanguage}
                  errors={
                    languages.some(
                      (language) => language.key === settings.uiLanguage.value
                    )
                      ? settings.uiLanguage.errors
                      : [
                          ...settings.uiLanguage.errors,
                          { message: translate('InvalidUILanguage') },
                        ]
                  }
                />
              </FormGroup>
            </FieldSet>
          </Form>
        ) : null}
      </PageContentBody>
    </PageContent>
  );
}

export default UISettings;
