// Monscript.js — gestion AJAX du panier (version fetch, sans jQuery)
(function () {
  const fmt = n => Number(n).toFixed(2);
  async function post(url) {
    const r = await fetch(url, { method: "POST", headers: { "X-Requested-With": "XMLHttpRequest" } });
    if (!r.ok) throw new Error(r.status);
    return r.json();
  }
  function bump(count) {
    const b = document.getElementById("cartCount");
    if (!b) return;
    b.textContent = count;
    if (window.gsap) gsap.fromTo(b, { scale: 1.6 }, { scale: 1, duration: 0.5, ease: "back.out(3)" });
  }
  function updateRow(id, d) {
    const q = document.getElementById("quantite_" + id);
    const t = document.getElementById("total_" + id);
    const tp = document.getElementById("totalPanier");
    const sc = document.getElementById("sumCount");
    if (tp) tp.textContent = fmt(d.total);
    if (sc) sc.textContent = d.count;
    bump(d.count);
    if (d.quantite === 0) {
      const row = document.getElementById("row_" + id);
      const done = () => { row.remove(); if (d.count === 0) location.reload(); };
      window.gsap ? gsap.to(row, { x: 60, opacity: 0, height: 0, padding: 0, margin: 0, duration: 0.4, onComplete: done }) : done();
      return;
    }
    if (q) q.textContent = d.quantite;
    if (t) t.textContent = fmt(d.totalRow);
  }
  document.addEventListener("click", async e => {
    const add = e.target.closest(".add-ajax");
    const plus = e.target.closest(".PlusProducts");
    const min = e.target.closest(".MinProducts");
    const rem = e.target.closest(".RemoveProduct");
    try {
      if (add) {
        add.disabled = true;
        const d = await post("/Panier/AddAjax/" + add.dataset.id);
        bump(d.count); window.showToast("« " + d.name + " » ajouté au panier");
        add.disabled = false;
      } else if (plus) updateRow(plus.dataset.id, await post("/Panier/PlusProduct/" + plus.dataset.id));
      else if (min) updateRow(min.dataset.id, await post("/Panier/MinusProduct/" + min.dataset.id));
      else if (rem) updateRow(rem.dataset.id, await post("/Panier/RemoveProduct/" + rem.dataset.id));
    } catch { window.showToast("Erreur, veuillez réessayer"); if (add) add.disabled = false; }
  });
})();
