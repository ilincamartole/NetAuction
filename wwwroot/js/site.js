// NetAuction Premium Interactions - 2026

document.addEventListener("DOMContentLoaded", function () {
    "use strict";

    // 1. EFECT DE NAVBAR "STICKY" CU BLUR
    const navbar = document.querySelector(".navbar-glass");
    window.addEventListener("scroll", () => {
        if (window.scrollY > 50) {
            navbar?.classList.add("shadow-lg", "py-2");
            navbar?.classList.remove("py-3");
        } else {
            navbar?.classList.remove("shadow-lg", "py-2");
            navbar?.classList.add("py-3");
        }
    });

    // 2. INIȚIALIZARE TOOLTIPS ȘI POPOVERS (Bootstrap)
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // 3. ANIMAȚIE DE TIP "REVEAL" LA SCROLL
    // Adaugă clasa 'reveal' pe elementele care vrei să apară treptat
    const revealElements = document.querySelectorAll(".reveal");
    const revealOnScroll = () => {
        const windowHeight = window.innerHeight;
        revealElements.forEach((el) => {
            const elementTop = el.getBoundingClientRect().top;
            const elementVisible = 150;
            if (elementTop < windowHeight - elementVisible) {
                el.classList.add("active");
            }
        });
    };
    window.addEventListener("scroll", revealOnScroll);

    // 4. MICRO-INTERACȚIUNE PENTRU BUTOANE (Efect de "Ripple")
    const buttons = document.querySelectorAll(".btn-primary, .btn-warning");
    buttons.forEach((btn) => {
        btn.addEventListener("mousedown", function (e) {
            let x = e.clientX - e.target.offsetLeft;
            let y = e.clientY - e.target.offsetTop;
            let ripples = document.createElement("span");
            ripples.style.left = x + "px";
            ripples.style.top = y + "px";
            ripples.classList.add("ripple-effect");
            this.appendChild(ripples);
            setTimeout(() => { ripples.remove(); }, 1000);
        });
    });

    // 5. AUTO-HIDE PENTRU ALERTE (FEEDBACK UX)
    const alerts = document.querySelectorAll(".alert-dismissible");
    alerts.forEach((alert) => {
        setTimeout(() => {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000); // Se închide automat după 5 secunde
    });

    // 6. LOADING STATE PE BUTOANE LA SUBMIT
    const forms = document.querySelectorAll("form");
    forms.forEach(form => {
        form.addEventListener("submit", function () {
            const submitBtn = this.querySelector('button[type="submit"]');
            if (submitBtn && !submitBtn.classList.contains('no-loader')) {
                submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span> Se procesează...';
                submitBtn.disabled = true;
            }
        });
    });
});