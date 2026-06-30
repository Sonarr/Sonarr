import {
  ArcElement,
  Chart,
  ChartEvent,
  ChartOptions,
  Legend,
  LegendElement,
  LegendItem,
  Tooltip,
  TooltipItem,
} from 'chart.js';
import React, { useMemo } from 'react';
import { Doughnut } from 'react-chartjs-2';
import { useUiSettingsValues } from 'Settings/UI/useUiSettings';
import useChartColors from 'Statistics/useChartColors';
import ChartContainer from './ChartContainer';
import createSlicePattern from './createSlicePattern';

Chart.register(ArcElement, Tooltip, Legend);

export interface DoughnutChartItem {
  label: string;
  value: number;
  color: string;
}

interface DoughnutChartProps {
  title: string;
  items: DoughnutChartItem[];
}

function DoughnutChart({ title, items }: DoughnutChartProps) {
  const colors = useChartColors();
  const { enableColorImpairedMode } = useUiSettingsValues();

  const data = useMemo(() => {
    return {
      labels: items.map((item) => item.label),
      datasets: [
        {
          data: items.map((item) => item.value),
          backgroundColor: items.map((item, index) =>
            enableColorImpairedMode
              ? createSlicePattern(
                  item.color,
                  colors.colorImpairedStripe,
                  index
                )
              : item.color
          ),
          borderWidth: 0,
        },
      ],
    };
  }, [colors, enableColorImpairedMode, items]);

  const options = useMemo<ChartOptions<'doughnut'>>(() => {
    return {
      responsive: true,
      maintainAspectRatio: false,
      cutout: '60%',
      plugins: {
        legend: {
          position: 'bottom',
          labels: {
            color: colors.text,
          },
          onHover: (
            event: ChartEvent,
            legendItem: LegendItem,
            legend: LegendElement<'doughnut'>
          ) => {
            const chart = legend.chart;
            const index = legendItem.index ?? 0;
            const activeElements = [{ datasetIndex: 0, index }];

            chart.setActiveElements(activeElements);
            chart.tooltip?.setActiveElements(activeElements, {
              x: event.x ?? 0,
              y: event.y ?? 0,
            });
            chart.update();
          },
          onLeave: (
            _event: ChartEvent,
            _legendItem: LegendItem,
            legend: LegendElement<'doughnut'>
          ) => {
            const chart = legend.chart;

            chart.setActiveElements([]);
            chart.tooltip?.setActiveElements([], { x: 0, y: 0 });
            chart.update();
          },
        },
        tooltip: {
          callbacks: {
            label: (context: TooltipItem<'doughnut'>) => {
              const total = items.reduce((acc, item) => acc + item.value, 0);
              const percentage = total
                ? Math.round((context.parsed / total) * 100)
                : 0;

              return ` ${context.parsed.toLocaleString()} (${percentage}%)`;
            },
          },
        },
      },
    };
  }, [colors, items]);

  return (
    <ChartContainer title={title}>
      <Doughnut data={data} options={options} />
    </ChartContainer>
  );
}

export default DoughnutChart;
