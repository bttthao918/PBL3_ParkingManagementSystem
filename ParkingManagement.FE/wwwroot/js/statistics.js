document.addEventListener("DOMContentLoaded", function () {
    if (typeof Chart === "undefined") {
        console.error("Chart.js chưa được load");
        return;
    }

    if (!window.statisticsConfig) {
        console.error("Thiếu statisticsConfig");
        return;
    }

    Chart.defaults.font.family = "'Segoe UI', sans-serif";
    Chart.defaults.font.size = 13;
    Chart.defaults.color = "#334155";

    const blue = "#0d8bff";
    const lightBlue = "rgba(13, 139, 255, 0.35)";
    const green = "#22c55e";
    const purple = "#6d28d9";
    const orange = "#ff7a00";

    const config = window.statisticsConfig;
    const formatCurrency = value => {
        const number = Number(value || 0);
        return number.toLocaleString("vi-VN") + " đ";
    };
    const lineLabels = config.line?.labels || [];
    const lineCurrent = config.line?.current || [];
    const linePrevious = config.line?.previous || [];
    const hasPreviousLine = linePrevious.length > 0;

    const lineCtx = document.getElementById("mainLineChart");
    const donutCtx = document.getElementById("mainDonutChart");
    const barCtx = document.getElementById("mainBarChart");

    if (lineCtx) {
        const datasets = [
            {
                label: "Kỳ hiện tại",
                data: lineCurrent,
                borderColor: blue,
                backgroundColor: "rgba(13, 139, 255, 0.14)",
                fill: true,
                tension: 0.45,
                pointRadius: 5,
                pointHoverRadius: 7
            }
        ];

        if (hasPreviousLine) {
            datasets.push({
                label: "Kỳ trước",
                data: linePrevious,
                borderColor: lightBlue,
                backgroundColor: "transparent",
                borderDash: [6, 6],
                fill: false,
                tension: 0.45,
                pointRadius: 4
            });
        }

        new Chart(lineCtx, {
            type: "line",
            data: {
                labels: lineLabels,
                datasets
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: {
                    mode: "index",
                    intersect: false
                },
                plugins: {
                    legend: {
                        labels: {
                            usePointStyle: true
                        }
                    },
                    tooltip: {
                        backgroundColor: "#ffffff",
                        titleColor: "#102033",
                        bodyColor: "#334155",
                        borderColor: "#d7e2f2",
                        borderWidth: 1,
                        padding: 14,
                        callbacks: {
                            title: context => context[0].label,
                            label: context => {
                                if (config.type === "revenue") {
                                    return `${context.dataset.label}: ${formatCurrency(context.raw)}`;
                                }

                                return `${context.dataset.label}: ${context.raw} khách`;
                            }
                        }
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            color: "#334155",
                            font: { size: 13, weight: "600" },
                            callback: value => config.type === "revenue" ? formatCurrency(value) : value
                        },
                        grid: { color: "#e2e8f0" }
                    },
                    x: {
                        ticks: {
                            color: "#334155",
                            font: { size: 13, weight: "600" }
                        },
                        grid: { display: false }
                    }
                }
            }
        });
    }

    if (donutCtx) {
        new Chart(donutCtx, {
            type: "doughnut",
            data: {
                labels: config.donut?.labels || [],
                datasets: [{
                    data: config.donut?.data || [],
                    backgroundColor: [blue, green, orange, purple],
                    borderWidth: 0,
                    cutout: "68%"
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: true,
                aspectRatio: 1,
                plugins: {
                    legend: { display: false }
                }
            }
        });
    }

    if (barCtx) {
        new Chart(barCtx, {
            type: "bar",
            data: {
                labels: config.bar?.labels || [],
                datasets: [{
                    data: config.bar?.data || [],
                    backgroundColor: [blue, green, purple, orange],
                    borderRadius: 8,
                    barThickness: 52
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            color: "#334155",
                            font: { size: 13, weight: "600" },
                            callback: value => config.type === "revenue" ? formatCurrency(value) : value
                        },
                        grid: { color: "#e2e8f0" }
                    },
                    x: {
                        ticks: {
                            color: "#334155",
                            font: { size: 13, weight: "600" }
                        },
                        grid: { display: false }
                    }
                }
            }
        });
    }
});
