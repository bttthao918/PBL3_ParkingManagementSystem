(function () {
    document.querySelectorAll("form[data-auto-filter]").forEach(setupAutoFilter);

    function setupAutoFilter(form) {
        const searchInput = form.querySelector("[data-auto-filter-search]");
        const controls = form.querySelectorAll("[data-auto-filter-control]");
        const targetSelector = form.dataset.autoFilterTarget || "[data-auto-filter-results]";
        let timer;
        let composing = false;
        let controller;

        function buildUrl(sourceForm = form) {
            const url = new URL(sourceForm.action || window.location.href, window.location.origin);
            const data = new FormData(sourceForm);

            for (const [name, value] of data.entries()) {
                const field = sourceForm.elements.namedItem(name);
                const filterValue = field instanceof HTMLElement && field.dataset.isoValue !== undefined
                    ? field.dataset.isoValue
                    : value;
                const text = String(filterValue).trim();
                if (text) {
                    url.searchParams.set(name, text);
                } else {
                    url.searchParams.delete(name);
                }
            }

            return url;
        }

        async function updateResults(url, updateHistory) {
            controller?.abort();
            controller = new AbortController();

            const currentTargets = Array.from(document.querySelectorAll(targetSelector));
            currentTargets.forEach(target => {
                target.classList.add("auto-filter-loading");
                target.setAttribute("aria-busy", "true");
            });

            try {
                const response = await fetch(url, {
                    headers: { "X-Requested-With": "XMLHttpRequest" },
                    signal: controller.signal
                });

                if (!response.ok) {
                    throw new Error(`Filter request failed: ${response.status}`);
                }

                const html = await response.text();
                const nextDocument = new DOMParser().parseFromString(html, "text/html");
                const nextTargets = Array.from(nextDocument.querySelectorAll(targetSelector));

                if (currentTargets.length === 0 || nextTargets.length !== currentTargets.length) {
                    throw new Error("Filter result region was not found.");
                }

                currentTargets.forEach((target, index) => {
                    target.innerHTML = nextTargets[index].innerHTML;
                    target.classList.remove("auto-filter-loading");
                    target.removeAttribute("aria-busy");
                });

                syncHiddenFilterValues(form);
                if (updateHistory) {
                    window.history.replaceState({}, "", url);
                }

                document.dispatchEvent(new CustomEvent("auto-filter:updated", {
                    detail: { form, url, targetSelector }
                }));
            } catch (error) {
                if (error.name === "AbortError") {
                    return;
                }

                currentTargets.forEach(target => {
                    target.classList.remove("auto-filter-loading");
                    target.removeAttribute("aria-busy");
                });

                window.showNotice?.(
                    "Không thể tải kết quả",
                    "Có lỗi khi tìm kiếm. Vui lòng thử lại.",
                    "danger"
                );
            }
        }

        function submitFilters() {
            updateResults(buildUrl(), true);
        }

        function scheduleSubmit() {
            window.clearTimeout(timer);
            timer = window.setTimeout(submitFilters, 350);
        }

        searchInput?.addEventListener("compositionstart", function () {
            composing = true;
        });

        searchInput?.addEventListener("compositionend", function () {
            composing = false;
            scheduleSubmit();
        });

        searchInput?.addEventListener("input", function () {
            if (!composing) {
                scheduleSubmit();
            }
        });

        controls.forEach(function (control) {
            control.addEventListener("change", submitFilters);
        });

        form.addEventListener("submit", function (event) {
            event.preventDefault();
            submitFilters();
        });

        document.addEventListener("click", function (event) {
            const link = event.target.closest("a");
            const pagination = link?.closest(".pagination, .pagination-row, .ticket-pagination, .emp-pagination");
            if (!link || !pagination || !link.closest(targetSelector) || link.target || link.hasAttribute("download")) {
                return;
            }

            const url = new URL(link.href, window.location.origin);
            if (url.origin !== window.location.origin) {
                return;
            }

            event.preventDefault();
            updateResults(url, true);
        });

        document.addEventListener("change", function (event) {
            const pageSizeForm = event.target.closest(".page-size-form");
            if (!pageSizeForm || !pageSizeForm.closest(targetSelector)) {
                return;
            }

            event.preventDefault();
            updateResults(buildUrl(pageSizeForm), true);
        });
    }

    function syncHiddenFilterValues(sourceForm) {
        new FormData(sourceForm).forEach(function (value, name) {
            document.getElementsByName(name).forEach(function (element) {
                if (element !== sourceForm.elements[name] && element instanceof HTMLInputElement && element.type === "hidden") {
                    element.value = String(value);
                }
            });
        });
    }
})();
