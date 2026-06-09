(function () {
    if (!window.EventSource) {
        return;
    }

    const routes = [
        { pattern: /^\/customer\/dashboard/i, topics: ["dashboard", "reservations", "tickets", "monthly-tickets", "payments", "parking-slots", "customers"] },
        { pattern: /^\/customer\/booking/i, topics: ["reservations", "parking-slots"] },
        { pattern: /^\/customer\/ticket/i, topics: ["tickets", "payments", "pricing", "parking-slots"] },
        { pattern: /^\/customer\/monthlyticket/i, topics: ["monthly-tickets", "payments", "pricing", "customers"] },
        { pattern: /^\/employee\/dashboard/i, topics: ["dashboard", "reservations", "tickets", "monthly-tickets", "payments", "parking-slots", "customers", "employees"] },
        { pattern: /^\/employee\/parkingoperation/i, topics: ["parking-slots", "reservations", "tickets", "payments", "monthly-tickets"] },
        { pattern: /^\/employee\/reservationmanagement/i, topics: ["reservations", "parking-slots"] },
        { pattern: /^\/employee\/ticketmanagement/i, topics: ["tickets", "payments", "pricing", "parking-slots"] },
        { pattern: /^\/employee\/monthlyticketmanagement/i, topics: ["monthly-tickets", "payments", "pricing", "customers"] },
        { pattern: /^\/employee\/parkingslotmanagement/i, topics: ["parking-slots", "reservations", "tickets"] },
        { pattern: /^\/employee\/customermanagement/i, topics: ["customers", "reservations", "tickets", "monthly-tickets", "payments"] },
        { pattern: /^\/employee\/shiftmanagement/i, topics: ["employees"] },
        { pattern: /^\/employee\/personalreport/i, topics: ["dashboard", "payments", "tickets", "reservations", "monthly-tickets"] },
        { pattern: /^\/employee\/revenuestatistics/i, topics: ["dashboard", "payments", "tickets", "monthly-tickets"] },
        { pattern: /^\/admin\/dashboard/i, topics: ["dashboard", "reservations", "tickets", "monthly-tickets", "payments", "parking-slots", "customers", "employees"] },
        { pattern: /^\/admin\/ticketmanagement/i, topics: ["tickets", "payments", "pricing", "parking-slots"] },
        { pattern: /^\/admin\/monthlyticketmanagement/i, topics: ["monthly-tickets", "payments", "pricing", "customers"] },
        { pattern: /^\/admin\/parkingslotmanagement/i, topics: ["parking-slots", "reservations", "tickets"] },
        { pattern: /^\/admin\/employeemanagement/i, topics: ["employees"] },
        { pattern: /^\/admin\/revenuestatistics/i, topics: ["dashboard", "payments", "tickets", "monthly-tickets"] },
        { pattern: /^\/admin\/customerstatistics/i, topics: ["dashboard", "customers", "reservations", "tickets", "monthly-tickets", "payments"] },
        { pattern: /^\/account\/profile/i, topics: ["customers", "employees"] }
    ];

    const route = routes.find(item => item.pattern.test(window.location.pathname));
    if (!route) {
        return;
    }

    let pendingReload = false;
    let reloadTimer = 0;
    let source = null;

    function isRelevant(update) {
        return update && route.topics.includes(update.Topic);
    }

    function isUserEditing() {
        const activeElement = document.activeElement;
        const activeTag = activeElement?.tagName?.toLowerCase();
        if (activeElement?.isContentEditable || ["input", "textarea", "select"].includes(activeTag)) {
            return true;
        }

        return Boolean(document.querySelector(
            "dialog[open], .modal.show, .modal.active, .modal[style*='display: block'], [data-realtime-pause='true']"
        ));
    }

    function reloadPage() {
        pendingReload = false;
        sessionStorage.setItem("parkingRealtimeReloadAt", String(Date.now()));
        window.location.reload();
    }

    function scheduleReload() {
        if (document.hidden || isUserEditing()) {
            pendingReload = true;
            window.clearTimeout(reloadTimer);
            reloadTimer = window.setTimeout(scheduleReload, 3000);
            return;
        }

        window.clearTimeout(reloadTimer);
        reloadTimer = window.setTimeout(reloadPage, 800);
    }

    document.addEventListener("visibilitychange", function () {
        if (!document.hidden && pendingReload) {
            scheduleReload();
        }
    });

    try {
        source = new EventSource("/realtime/stream");

        source.addEventListener("parking-update", function (event) {
            try {
                const update = JSON.parse(event.data);
                if (isRelevant(update)) {
                    scheduleReload();
                }
            } catch {
                scheduleReload();
            }
        });

        source.addEventListener("parking-error", function () {
            source?.close();
            source = null;
        });

        window.addEventListener("beforeunload", function () {
            source?.close();
        });
    } catch {
        source?.close();
    }
})();

