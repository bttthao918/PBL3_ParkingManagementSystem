document.addEventListener("DOMContentLoaded", function () {
    const data = window.personalReportData;

    if (!data) return;

    createRevenueChart(data.labels, data.revenues);
    createTicketChart(data.labels, data.tickets);
});

function createRevenueChart(labels, values) {
    const ctx = document.getElementById("revenueChart");

    if (!ctx) return;

    new Chart(ctx, {
        type: "line",
        data: {
            labels: labels,
            datasets: [
                {
                    label: "Doanh thu",
                    data: values,
                    tension: 0.4,
                    fill: true,
                    borderColor: "#1479ff",
                    backgroundColor: "rgba(20, 121, 255, 0.10)",
                    borderWidth: 3,
                    pointRadius: 4,
                    pointHoverRadius: 6,
                    pointBackgroundColor: "#1479ff",
                    pointBorderColor: "#ffffff",
                    pointBorderWidth: 2
                }
            ]
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
                    display: false
                },
                tooltip: {
                    backgroundColor: "#ffffff",
                    titleColor: "#10233f",
                    bodyColor: "#10233f",
                    borderColor: "#e3ebf6",
                    borderWidth: 1,
                    padding: 12,
                    displayColors: true,
                    callbacks: {
                        label: function (context) {
                            const value = context.raw || 0;
                            return "Doanh thu: " + formatCurrency(value);
                        }
                    }
                }
            },

            scales: {
                x: {
                    grid: {
                        display: false
                    },
                    ticks: {
                        color: "#71809a",
                        font: {
                            size: 12,
                            family: "Inter, Arial, sans-serif"
                        }
                    }
                },
                y: {
                    beginAtZero: true,
                    grid: {
                        color: "#edf2f8"
                    },
                    ticks: {
                        color: "#71809a",
                        callback: function (value) {
                            return formatMillion(value);
                        },
                        font: {
                            size: 12,
                            family: "Inter, Arial, sans-serif"
                        }
                    }
                }
            }
        }
    });
}

function createTicketChart(labels, values) {
    const ctx = document.getElementById("ticketChart");

    if (!ctx) return;

    new Chart(ctx, {
        type: "line",
        data: {
            labels: labels,
            datasets: [
                {
                    label: "Số vé",
                    data: values,
                    tension: 0.4,
                    fill: true,
                    borderColor: "#16a34a",
                    backgroundColor: "rgba(22, 163, 74, 0.10)",
                    borderWidth: 3,
                    pointRadius: 4,
                    pointHoverRadius: 6,
                    pointBackgroundColor: "#16a34a",
                    pointBorderColor: "#ffffff",
                    pointBorderWidth: 2
                }
            ]
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
                    display: false
                },
                tooltip: {
                    backgroundColor: "#ffffff",
                    titleColor: "#10233f",
                    bodyColor: "#10233f",
                    borderColor: "#e3ebf6",
                    borderWidth: 1,
                    padding: 12,
                    displayColors: true,
                    callbacks: {
                        label: function (context) {
                            const value = context.raw || 0;
                            return "Số vé: " + value + " vé";
                        }
                    }
                }
            },

            scales: {
                x: {
                    grid: {
                        display: false
                    },
                    ticks: {
                        color: "#71809a",
                        font: {
                            size: 12,
                            family: "Inter, Arial, sans-serif"
                        }
                    }
                },
                y: {
                    beginAtZero: true,
                    grid: {
                        color: "#edf2f8"
                    },
                    ticks: {
                        color: "#71809a",
                        stepSize: 20,
                        font: {
                            size: 12,
                            family: "Inter, Arial, sans-serif"
                        }
                    }
                }
            }
        }
    });
}

function formatCurrency(value) {
    return new Intl.NumberFormat("vi-VN").format(value) + " đ";
}

function formatMillion(value) {
    if (value >= 1000000) {
        return value / 1000000 + "M";
    }

    return value;
}