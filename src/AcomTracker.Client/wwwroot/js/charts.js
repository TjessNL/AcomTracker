// Chart.js interop helpers for Blazor
window.acomCharts = (() => {
    const registry = {};

    function destroy(id) {
        if (registry[id]) {
            registry[id].destroy();
            delete registry[id];
        }
    }

    function create(id, type, labels, datasets, options) {
        destroy(id);
        const canvas = document.getElementById(id);
        if (!canvas) return;
        registry[id] = new Chart(canvas, {
            type,
            data: { labels, datasets },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { position: 'bottom', labels: { font: { size: 12 }, padding: 16 } },
                    ...options?.plugins
                },
                ...options
            }
        });
    }

    return {
        renderBar(id, labels, datasets) {
            create(id, 'bar', labels, datasets, {
                plugins: { legend: { display: datasets.length > 1 } },
                scales: {
                    x: { grid: { display: false } },
                    y: { beginAtZero: true, ticks: { callback: v => '€' + v.toLocaleString() } }
                }
            });
        },

        renderDoughnut(id, labels, data, colors) {
            create(id, 'doughnut', labels, [{
                data,
                backgroundColor: colors,
                borderWidth: 2,
                borderColor: '#fff'
            }], {
                cutout: '65%',
                plugins: { legend: { position: 'bottom' } }
            });
        },

        renderHorizontalBar(id, labels, data, color) {
            create(id, 'bar', labels, [{
                data,
                backgroundColor: color,
                borderRadius: 4
            }], {
                indexAxis: 'y',
                plugins: { legend: { display: false } },
                scales: {
                    x: { beginAtZero: true, ticks: { callback: v => '€' + v.toLocaleString() } },
                    y: { grid: { display: false } }
                }
            });
        },

        destroyChart(id) { destroy(id); }
    };
})();
