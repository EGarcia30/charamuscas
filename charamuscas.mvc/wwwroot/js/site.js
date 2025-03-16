// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Funciones auxiliares para formatear moneda y número
function formatCurrency(number) {
    return new Intl.NumberFormat('es-MX', { // Ajusta la localización si es necesario
        style: 'currency',
        currency: 'MXN' // Ajusta la moneda si es necesario
    }).format(number);
}

function formatNumber(number) {
    return new Intl.NumberFormat('es-MX', { // Ajusta la localización si es necesario
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    }).format(number);
}

function formatPercent(number) {
    return new Intl.NumberFormat('es-MX', { // Ajusta la localización si es necesario
        style: 'percent',
        minimumFractionDigits: 0,
        maximumFractionDigits: 0
    }).format(number);
}
