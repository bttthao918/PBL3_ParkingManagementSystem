document.addEventListener("DOMContentLoaded", function () {
    if (typeof Chart === "undefined") {
        console.error("Chart.js chua duoc load");
        return;
    }

    const config = window.statisticsConfig || {};

    Chart.defaults.font.family = "'Segoe UI', sans-serif";
    Chart.defaults.font.size = 13;
    Chart.defaults.color = "#334155";

    const colors = {
        blue: "#0d8bff",
        lightBlue: "rgba(13, 139, 255, 0.35)",
        green: "#22c55e",
        purple: "#6d28d9",
        orange: "#ff7a00",
        cyan: "#06b6d4",
        empty: "#cbd5e1"
    };

    const palette = [colors.blue, colors.green, colors.orange, colors.purple, colors.cyan];
    const numberFormatter = new Intl.NumberFormat("vi-VN");
    const toArray = value => Array.isArray(value) ? value : [];
    const toNumbers = values => toArray(values).map(value => Number(value || 0));
    const hasPositiveValue = values => values.some(value => Number(value) > 0);
    const formatCurrency = value => numberFormatter.format(Number(value || 0)) + " đ";
    const formatPlainNumber = value => numberFormatter.format(Number(value || 0));
    const formatMetric = value => config.type === "revenue"
        ? formatCurrency(value)
        : formatPlainNumber(value);
    const formatAxisValue = value => {
        const number = Number(value || 0);
        if (config.type !== "revenue") {
            return numberFormatter.format(number);
        }

        if (Math.abs(number) >= 1000000000) return (number / 1000000000).toFixed(1).replace(".0", "") + "B";
        if (Math.abs(number) >= 1000000) return (number / 1000000).toFixed(1).replace(".0", "") + "M";
        if (Math.abs(number) >= 1000) return (number / 1000).toFixed(0) + "K";
        return numberFormatter.format(number);
    };

    const lineLabels = toArray(config.line?.labels).map(String);
    const lineCurrent = toNumbers(config.line?.current);
    const linePrevious = toNumbers(config.line?.previous);
    const hasLineData = lineLabels.length > 0 && lineCurrent.length > 0;
    const displayLineLabels = hasLineData ? lineLabels : ["Chưa có dữ liệu"];
    const displayLineCurrent = hasLineData
        ? lineLabels.map((_, index) => lineCurrent[index] ?? 0)
        : [0];

    const lineCtx = document.getElementById("mainLineChart");
    const donutCtx = document.getElementById("mainDonutChart");
    const barCtx = document.getElementById("mainBarChart");

    if (lineCtx) {
        const datasets = [
            {
                label: "Kỳ hiện tại",
                data: displayLineCurrent,
                borderColor: colors.blue,
                backgroundColor: "rgba(13, 139, 255, 0.14)",
                fill: true,
                tension: 0.42,
                pointRadius: 4,
                pointHoverRadius: 6,
                pointBackgroundColor: colors.blue,
                pointBorderColor: "#ffffff",
                pointBorderWidth: 2
            }
        ];

        if (hasLineData && linePrevious.length > 0) {
            datasets.push({
                label: "Kỳ trước",
                data: lineLabels.map((_, index) => linePrevious[index] ?? 0),
                borderColor: colors.lightBlue,
                backgroundColor: "transparent",
                borderDash: [6, 6],
                fill: false,
                tension: 0.42,
                pointRadius: 3,
                pointHoverRadius: 5
            });
        }

        new Chart(lineCtx, {
            type: "line",
            data: {
                labels: displayLineLabels,
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
                            title: context => context[0]?.label || "",
                            label: context => `${context.dataset.label}: ${formatMetric(context.raw)}`
                        }
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            color: "#334155",
                            font: { size: 13, weight: "600" },
                            callback: value => formatAxisValue(value)
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
        const donutLabels = toArray(config.donut?.labels).map(String);
        const donutData = toNumbers(config.donut?.data);
        const hasDonutData = donutLabels.length > 0 && hasPositiveValue(donutData);

        new Chart(donutCtx, {
            type: "doughnut",
            data: {
                labels: hasDonutData ? donutLabels : ["Chưa có dữ liệu"],
                datasets: [{
                    data: hasDonutData ? donutData : [1],
                    backgroundColor: hasDonutData ? palette : [colors.empty],
                    borderWidth: 0,
                    cutout: "68%"
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: true,
                aspectRatio: 1,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        callbacks: {
                            label: context => hasDonutData
                                ? `${context.label}: ${formatMetric(context.raw)}`
                                : "Chưa có dữ liệu"
                        }
                    }
                }
            }
        });
    }

    if (barCtx) {
        const barLabels = toArray(config.bar?.labels).map(String);
        const barData = toNumbers(config.bar?.data);
        const hasBarData = barLabels.length > 0 && barData.length > 0;

        new Chart(barCtx, {
            type: "bar",
            data: {
                labels: hasBarData ? barLabels : ["Chưa có dữ liệu"],
                datasets: [{
                    data: hasBarData ? barData : [0],
                    backgroundColor: hasBarData ? palette : [colors.empty],
                    borderRadius: 8,
                    maxBarThickness: 52
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        callbacks: {
                            label: context => formatMetric(context.raw)
                        }
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            color: "#334155",
                            font: { size: 13, weight: "600" },
                            callback: value => formatAxisValue(value)
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
