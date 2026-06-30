import { useMemo } from 'react';
import { useThemeColor } from 'Helpers/Hooks/useTheme';

export interface ChartColors {
  text: string;
  grid: string;
  bar: string;
  palette: string[];
  successColor: string;
  dangerColor: string;
  warningColor: string;
  grayColor: string;
  colorImpairedStripe: string;
}

const useChartColors = (): ChartColors => {
  const textColor = useThemeColor('textColor');
  const darkGray = useThemeColor('darkGray');
  const gray = useThemeColor('gray');
  const themeBlue = useThemeColor('themeBlue');
  const themeAlternateBlue = useThemeColor('themeAlternateBlue');
  const successColor = useThemeColor('successColor');
  const dangerColor = useThemeColor('dangerColor');
  const warningColor = useThemeColor('warningColor');
  const purple = useThemeColor('purple');
  const pink = useThemeColor('pink');
  const colorImpairedGradient = useThemeColor('colorImpairedGradient');

  return useMemo(() => {
    return {
      text: textColor,
      grid: `${darkGray}40`,
      bar: themeBlue,
      palette: [
        themeBlue,
        successColor,
        warningColor,
        dangerColor,
        purple,
        pink,
        themeAlternateBlue,
        gray,
      ],
      successColor,
      dangerColor,
      warningColor,
      grayColor: gray,
      colorImpairedStripe: colorImpairedGradient,
    };
  }, [
    textColor,
    darkGray,
    gray,
    themeBlue,
    themeAlternateBlue,
    successColor,
    dangerColor,
    warningColor,
    purple,
    pink,
    colorImpairedGradient,
  ]);
};

export default useChartColors;
