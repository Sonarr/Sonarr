import {
  BarElement,
  CategoryScale,
  Chart,
  ChartOptions,
  LinearScale,
  Tooltip,
  TooltipItem,
} from 'chart.js';
import React, { useMemo } from 'react';
import { Bar } from 'react-chartjs-2';
import useChartColors from 'Statistics/useChartColors';
import ChartContainer from './ChartContainer';

Chart.register(BarElement, CategoryScale, LinearScale, Tooltip);

export interface BarChartItem {
  label: string;
  value: number;
  tooltipLines?: string[];
}

interface BarChartProps {
  title: string;
  items: BarChartItem[];
}

function BarChart({ title, items }: BarChartProps) {
  const colors = useChartColors();

  const data = useMemo(() => {
    return {
      labels: items.map((item) => item.label),
      datasets: [
        {
          data: items.map((item) => item.value),
          backgroundColor: colors.bar,
          borderRadius: 3,
          maxBarThickness: 50,
        },
      ],
    };
  }, [colors, items]);

  const options = useMemo<ChartOptions<'bar'>>(() => {
    return {
      responsive: true,
      maintainAspectRatio: false,
      scales: {
        x: {
          ticks: {
            color: colors.text,
          },
          grid: {
            display: false,
          },
        },
        y: {
          beginAtZero: true,
          ticks: {
            precision: 0,
            color: colors.text,
          },
          grid: {
            color: colors.grid,
          },
        },
      },
      plugins: {
        legend: {
          display: false,
        },
        tooltip: {
          callbacks: {
            label: (context: TooltipItem<'bar'>) => {
              return ` ${(context.parsed.y ?? 0).toLocaleString()}`;
            },
            afterBody: (contexts: TooltipItem<'bar'>[]) => {
              const index = contexts[0]?.dataIndex ?? 0;

              return items[index]?.tooltipLines ?? [];
            },
          },
        },
      },
    };
  }, [colors, items]);

  return (
    <ChartContainer title={title}>
      <Bar data={data} options={options} />
    </ChartContainer>
  );
}

export default BarChart;
