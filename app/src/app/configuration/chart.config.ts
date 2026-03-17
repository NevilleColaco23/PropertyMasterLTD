// Chart.js configuration and registration
import { Chart, registerables } from 'chart.js';

// Register all Chart.js components
Chart.register(...registerables);

// Global default configuration
Chart.defaults.font.family = "'Roboto', 'Helvetica', 'Arial', sans-serif";
Chart.defaults.color = '#666';
Chart.defaults.plugins.tooltip.backgroundColor = 'rgba(0, 0, 0, 0.8)';
Chart.defaults.plugins.tooltip.cornerRadius = 6;
Chart.defaults.plugins.tooltip.padding = 12;

export { Chart };
