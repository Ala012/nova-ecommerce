// Animations : Lenis (smooth scroll) + GSAP/ScrollTrigger + effets inspirés de React Bits
document.addEventListener("DOMContentLoaded", function () {
  const reduce = matchMedia("(prefers-reduced-motion: reduce)").matches;

  // Lenis smooth scroll
  if (window.Lenis && !reduce) {
    const lenis = new Lenis({ lerp: 0.1 });
    function raf(t) { lenis.raf(t); requestAnimationFrame(raf); }
    requestAnimationFrame(raf);
    if (window.ScrollTrigger) lenis.on("scroll", ScrollTrigger.update);
  }

  const sidebar = document.getElementById("sidebar");
  const overlay = document.getElementById("overlay");
  const menuButton = document.getElementById("toggleMenu");
  const closeButton = document.getElementById("closeMenu");
  let sidebarOpen = false;

  function setSidebarOpen(isOpen) {
    sidebarOpen = isOpen;
    sidebar.classList.toggle("open", isOpen);
    overlay.classList.toggle("show", isOpen);
    sidebar.setAttribute("aria-hidden", String(!isOpen));
    menuButton.setAttribute("aria-expanded", String(isOpen));
    document.body.classList.toggle("sidebar-open", isOpen);
  }

  menuButton.addEventListener("click", () => setSidebarOpen(!sidebarOpen));
  closeButton.addEventListener("click", () => setSidebarOpen(false));
  overlay.addEventListener("click", () => setSidebarOpen(false));
  document.addEventListener("keydown", event => {
    if (event.key === "Escape" && sidebarOpen) setSidebarOpen(false);
  });

  if (!window.gsap || reduce) return;
  gsap.registerPlugin(ScrollTrigger);

  // SplitText (style React Bits) : titres révélés mot par mot
  document.querySelectorAll(".split").forEach(el => {
    el.innerHTML = el.textContent.trim().split(/\s+/).map(w => `<span class="w"><span>${w}</span></span>`).join(" ");
    gsap.from(el.querySelectorAll(".w > span"), { yPercent: 110, duration: 0.9, ease: "power4.out", stagger: 0.06 });
  });
  gsap.from(".hero .lead, .hero .eyebrow, .toolbar", { y: 20, opacity: 0, duration: 0.8, delay: 0.3, stagger: 0.1 });

  // Reveal au scroll
  gsap.utils.toArray(".reveal").forEach((el, i) => {
    gsap.from(el, { y: 40, opacity: 0, duration: 0.8, ease: "power3.out", delay: (i % 4) * 0.06,
      scrollTrigger: { trigger: el, start: "top 92%" } });
  });

  // Spotlight card (React Bits)
  document.querySelectorAll(".spotlight").forEach(c => c.addEventListener("mousemove", e => {
    const r = c.getBoundingClientRect();
    c.style.setProperty("--mx", e.clientX - r.left + "px");
    c.style.setProperty("--my", e.clientY - r.top + "px");
  }));

  // Boutons magnétiques
  if (matchMedia("(hover: hover)").matches) document.querySelectorAll(".magnetic").forEach(b => {
    b.addEventListener("mousemove", e => {
      const r = b.getBoundingClientRect();
      gsap.to(b, { x: (e.clientX - r.left - r.width / 2) * 0.25, y: (e.clientY - r.top - r.height / 2) * 0.35, duration: 0.3 });
    });
    b.addEventListener("mouseleave", () => gsap.to(b, { x: 0, y: 0, duration: 0.5, ease: "elastic.out(1,0.4)" }));
  });

});

// Toast global
window.showToast = function (msg) {
  const t = document.getElementById("toast");
  t.textContent = msg; t.classList.add("show");
  clearTimeout(window._tt); window._tt = setTimeout(() => t.classList.remove("show"), 2400);
};
document.addEventListener("DOMContentLoaded", () => {
  const t = document.getElementById("toast");
  if (t.textContent.trim()) window.showToast(t.textContent.trim());
});
