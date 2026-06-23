/* @ds-bundle: {"format":3,"namespace":"KeorsoftMeridianUIDesignSystem_9d8388","components":[],"sourceHashes":{"ui_kits/shendevour-web/components/Atoms.jsx":"e386b0d311a6","ui_kits/shendevour-web/components/Dashboard.jsx":"2b178dcb0743","ui_kits/shendevour-web/components/Reproceso.jsx":"20f7ece6763e","ui_kits/shendevour-web/components/Restaurant.jsx":"542ab6b12451","ui_kits/shendevour-web/components/Shell.jsx":"8b688448a0de"},"inlinedExternals":[],"unexposedExports":[]} */

(() => {

const __ds_ns = (window.KeorsoftMeridianUIDesignSystem_9d8388 = window.KeorsoftMeridianUIDesignSystem_9d8388 || {});

const __ds_scope = {};

(__ds_ns.__errors = __ds_ns.__errors || []);

// ui_kits/shendevour-web/components/Atoms.jsx
try { (() => {
/* eslint-disable */
// ─────────────────────────────────────────────────────────────
// MeridianUI atoms — Icon, Chip, Button helpers
// ─────────────────────────────────────────────────────────────

function MIcon({
  name,
  size,
  color,
  className,
  style,
  fill
}) {
  const s = {
    fontSize: size,
    color,
    ...style
  };
  if (fill) s.fontVariationSettings = "'FILL' 1, 'wght' 500, 'GRAD' 0, 'opsz' 24";
  return /*#__PURE__*/React.createElement("span", {
    className: "mi material-symbols-rounded " + (className || ""),
    style: s
  }, name);
}
function Chip({
  kind,
  icon,
  children
}) {
  return /*#__PURE__*/React.createElement("span", {
    className: "chip " + (kind || "")
  }, icon && /*#__PURE__*/React.createElement(MIcon, {
    name: icon
  }), children);
}
function Checkbox({
  checked,
  disabled,
  onChange
}) {
  return /*#__PURE__*/React.createElement("span", {
    className: "cb" + (checked ? " checked" : "") + (disabled ? " dis" : ""),
    onClick: () => !disabled && onChange && onChange(!checked),
    role: "checkbox",
    "aria-checked": checked
  });
}

// Format helpers — Mexican peso, tabular nums
function money(n, opts = {}) {
  const sign = n < 0 ? "– " : "";
  const abs = Math.abs(n);
  return sign + "$ " + abs.toLocaleString("es-MX", {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2
  });
}
function moneyShort(n) {
  const sign = n < 0 ? "– " : "";
  const abs = Math.abs(n);
  return sign + "$ " + abs.toLocaleString("es-MX", {
    maximumFractionDigits: 0
  });
}
Object.assign(window, {
  MIcon,
  Chip,
  Checkbox,
  money,
  moneyShort
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/shendevour-web/components/Atoms.jsx", error: String((e && e.message) || e) }); }

// ui_kits/shendevour-web/components/Dashboard.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* eslint-disable */
// ─────────────────────────────────────────────────────────────
// Dashboard view — Resumen del Día
//   4 KPI cards · 4 table cards · 1 caja footer
// ─────────────────────────────────────────────────────────────

const dashStyles = {
  root: {
    display: "flex",
    flexDirection: "column",
    minHeight: "100%"
  },
  kpiRow: {
    display: "grid",
    gridTemplateColumns: "repeat(4, 1fr)",
    padding: "4px 10px"
  },
  tabRow: {
    display: "grid",
    gridTemplateColumns: "repeat(4, 1fr)",
    padding: "2px 10px",
    flex: 1,
    minHeight: 320
  },
  cajaWrap: {
    padding: "4px 16px 12px"
  }
};
const KPI_DATA = [{
  label: "LLEGADAS",
  icon: "flight_land",
  color: "#10B981",
  number: 17,
  segs: [{
    val: 6,
    color: "#10B981",
    legend: "Directos"
  }, {
    val: 8,
    color: "#34D399",
    legend: "Reserva"
  }, {
    val: 3,
    color: "#6EE7B7",
    legend: "Probables"
  }]
}, {
  label: "SALIDAS",
  icon: "flight_takeoff",
  color: "#F59E0B",
  number: 12,
  segs: [{
    val: 7,
    color: "#F59E0B",
    legend: "Realizadas"
  }, {
    val: 2,
    color: "#FBB95A",
    legend: "Inesperadas"
  }, {
    val: 3,
    color: "#FDE68A",
    legend: "Programadas"
  }]
}, {
  label: "OCUPACIÓN",
  icon: "bed",
  color: "#6366F1",
  number: 86,
  suffix: "%",
  badge: {
    text: "+12%",
    bg: "#E0E7FF",
    color: "#6366F1"
  },
  segs: [{
    val: 78,
    color: "#6366F1",
    legend: "Ocupadas"
  }, {
    val: 4,
    color: "#A5B4FC",
    legend: "Uso casa"
  }, {
    val: 4,
    color: "#EC4899",
    legend: "Bloqueadas"
  }, {
    val: 14,
    color: "#E5E7EB",
    legend: "Libres"
  }]
}, {
  label: "FORECAST",
  icon: "pending_actions",
  color: "#8B5CF6",
  number: 94,
  suffix: "%",
  badge: {
    text: "94%",
    bg: "#EDE9FE",
    color: "#8B5CF6"
  },
  segs: [{
    val: 86,
    color: "#8B5CF6",
    legend: "Prob. ocupadas"
  }, {
    val: 4,
    color: "#C4B5FD",
    legend: "Uso casa"
  }, {
    val: 4,
    color: "#EC4899",
    legend: "Bloqueadas"
  }, {
    val: 6,
    color: "#E5E7EB",
    legend: "Libres"
  }]
}];
function KpiCard({
  data
}) {
  const totalSeg = data.segs.reduce((a, s) => a + s.val, 0) || 1;
  return /*#__PURE__*/React.createElement("div", {
    style: {
      background: "#fff",
      borderRadius: 20,
      margin: 6,
      boxShadow: "var(--shadow)",
      overflow: "hidden",
      position: "relative"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      height: 4,
      background: data.color
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      padding: "12px 18px 16px"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      justifyContent: "space-between",
      marginBottom: 4
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 11,
      fontWeight: 600,
      color: "var(--gray-muted)",
      textTransform: "uppercase",
      letterSpacing: ".06em"
    }
  }, data.label), /*#__PURE__*/React.createElement(MIcon, {
    name: data.icon,
    size: 22,
    color: data.color
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "flex-end",
      gap: 10,
      margin: "4px 0 0"
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 42,
      fontWeight: 700,
      lineHeight: 1,
      color: data.color
    }
  }, data.number, data.suffix), data.badge && /*#__PURE__*/React.createElement("span", {
    style: {
      borderRadius: 10,
      padding: "3px 9px",
      fontSize: 13,
      fontWeight: 700,
      background: data.badge.bg,
      color: data.badge.color,
      marginBottom: 3
    }
  }, data.badge.text)), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      height: 10,
      borderRadius: 6,
      overflow: "hidden",
      margin: "12px 0 10px",
      gap: 1
    }
  }, data.segs.map((s, i) => /*#__PURE__*/React.createElement("span", {
    key: i,
    style: {
      flex: s.val / totalSeg,
      background: s.color,
      borderTopLeftRadius: i === 0 ? 5 : 0,
      borderBottomLeftRadius: i === 0 ? 5 : 0,
      borderTopRightRadius: i === data.segs.length - 1 ? 5 : 0,
      borderBottomRightRadius: i === data.segs.length - 1 ? 5 : 0
    }
  }))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      flexWrap: "wrap",
      gap: "6px 12px"
    }
  }, data.segs.map((s, i) => /*#__PURE__*/React.createElement("span", {
    key: i,
    style: {
      display: "flex",
      alignItems: "center",
      gap: 5,
      fontSize: 11.5,
      color: "var(--gray-muted)"
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      width: 9,
      height: 9,
      borderRadius: "50%",
      background: s.color
    }
  }), s.legend, " ", s.val)))));
}
function TableCard({
  color,
  bgPale,
  bg,
  label,
  sub,
  count,
  columns,
  rows,
  totalLabel,
  total
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      background: "#fff",
      borderRadius: 14,
      margin: 6,
      boxShadow: "var(--shadow)",
      display: "flex",
      flexDirection: "column",
      overflow: "hidden",
      minHeight: 0
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      padding: "10px 14px",
      background: bg,
      display: "flex",
      alignItems: "center",
      justifyContent: "space-between"
    }
  }, /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 10.5,
      fontWeight: 600,
      color: "var(--gray-muted)",
      textTransform: "uppercase",
      letterSpacing: ".06em"
    }
  }, label), /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 12,
      color,
      fontWeight: 500,
      marginTop: 2
    }
  }, sub)), /*#__PURE__*/React.createElement("div", {
    style: {
      background: color,
      color: "#fff",
      padding: "3px 10px",
      borderRadius: 10,
      fontSize: 11,
      fontWeight: 700
    }
  }, count)), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      overflow: "auto"
    }
  }, /*#__PURE__*/React.createElement("table", {
    style: {
      width: "100%",
      borderCollapse: "collapse",
      fontSize: 12.5
    }
  }, /*#__PURE__*/React.createElement("thead", null, /*#__PURE__*/React.createElement("tr", null, columns.map((c, i) => /*#__PURE__*/React.createElement("th", {
    key: i,
    style: {
      position: "sticky",
      top: 0,
      background: "#fff",
      padding: "7px 10px",
      textAlign: c.right ? "right" : c.center ? "center" : "left",
      fontSize: 10.5,
      fontWeight: 600,
      color: "var(--gray-muted)",
      borderBottom: "1px solid var(--gray-line)",
      textTransform: "uppercase",
      letterSpacing: ".05em"
    }
  }, c.label)))), /*#__PURE__*/React.createElement("tbody", null, rows.map((r, i) => /*#__PURE__*/React.createElement("tr", {
    key: i,
    style: {
      borderBottom: ".5px solid var(--gray-line)"
    },
    onMouseEnter: e => e.currentTarget.style.background = "#FAFAFA",
    onMouseLeave: e => e.currentTarget.style.background = ""
  }, r.map((cell, j) => /*#__PURE__*/React.createElement("td", {
    key: j,
    style: {
      padding: "6px 10px",
      textAlign: columns[j].right ? "right" : columns[j].center ? "center" : "left",
      fontWeight: columns[j].right ? 600 : 400,
      color: "#374151",
      fontVariantNumeric: columns[j].right ? "tabular-nums" : "normal"
    }
  }, cell))))))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      justifyContent: "space-between",
      padding: "8px 14px",
      background: "var(--gray-foot)"
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 11,
      color: "var(--gray-muted)"
    }
  }, totalLabel), /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 13,
      fontWeight: 700,
      color,
      fontVariantNumeric: "tabular-nums"
    }
  }, money(total))));
}
const TABLES = [{
  color: "#10B981",
  bg: "#F0FDF4",
  label: "REGISTROS DEL TURNO",
  sub: "habitaciones ocupadas hoy",
  count: 6,
  columns: [{
    label: "Hab.",
    center: true
  }, {
    label: "Huésped"
  }, {
    label: "Tarifa",
    right: true
  }],
  rows: [["318", "García L., Luis A.", money(2400)], ["112", "Robles, Ana", money(1800)], ["206", "Méndez, Roberto", money(2100)], ["404", "Hernández, María", money(1950)], ["210", "Torres, Patricia", money(1800)], ["301", "Villanueva, José", money(2100)]],
  totalLabel: "Total tarifas",
  total: 12150
}, {
  color: "#F59E0B",
  bg: "#FFFBEB",
  label: "RENTAS / EXTRAS",
  sub: "movs. de habitación",
  count: 4,
  columns: [{
    label: "Cuenta"
  }, {
    label: "Movs.",
    center: true
  }, {
    label: "Total",
    right: true
  }],
  rows: [["318 · García L.", "2", money(4400)], ["404 · Hernández", "1", money(1950)], ["210 · Torres", "1", money(1800)], ["112 · Robles", "1", money(1240)]],
  totalLabel: "Total rentas",
  total: 9390
}, {
  color: "#6366F1",
  bg: "#EEF2FF",
  label: "CARGOS",
  sub: "consumos y servicios",
  count: 5,
  columns: [{
    label: "Cuenta"
  }, {
    label: "Movs.",
    center: true
  }, {
    label: "Total",
    right: true
  }],
  rows: [["318 · García L.", "3", money(820)], ["206 · Méndez", "2", money(620)], ["112 · Robles", "1", money(180)], ["301 · Villanueva", "2", money(540)], ["210 · Torres", "1", money(220)]],
  totalLabel: "Total cargos",
  total: 2380
}, {
  color: "#8B5CF6",
  bg: "#F5F3FF",
  label: "ABONOS",
  sub: "pagos recibidos",
  count: 5,
  columns: [{
    label: "Cuenta"
  }, {
    label: "Movs.",
    center: true
  }, {
    label: "Total",
    right: true
  }],
  rows: [["318 · García L.", "1", money(2400)], ["404 · Hernández", "1", money(1950)], ["112 · Robles", "1", money(1800)], ["206 · Méndez", "1", money(2100)], ["301 · Villanueva", "1", money(2100)]],
  totalLabel: "Total abonos",
  total: 10350
}];
function CajaFooter() {
  const items = [{
    icon: "payments",
    lbl: "Efectivo",
    val: 18420,
    color: "#10B981",
    bg: "#DCFCE7"
  }, {
    icon: "credit_card",
    lbl: "Tarjetas y otros",
    val: 26310,
    color: "#6366F1",
    bg: "#E0E7FF"
  }, {
    icon: "shopping_bag",
    lbl: "Gastos / Retiros",
    val: -1240,
    color: "#F97316",
    bg: "#FFEDD5"
  }];
  const total = 18420 + 26310 - 1240;
  return /*#__PURE__*/React.createElement("div", {
    style: {
      background: "#fff",
      borderRadius: 18,
      boxShadow: "var(--shadow)",
      display: "flex",
      alignItems: "center",
      padding: "6px 8px"
    }
  }, items.map((it, i) => /*#__PURE__*/React.createElement("div", {
    key: i,
    style: {
      flex: 1,
      display: "flex",
      alignItems: "center",
      gap: 12,
      padding: "10px 16px"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      background: it.bg,
      borderRadius: 10,
      padding: 10,
      display: "flex"
    }
  }, /*#__PURE__*/React.createElement(MIcon, {
    name: it.icon,
    size: 20,
    color: it.color
  })), /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 10,
      fontWeight: 600,
      color: "var(--gray-muted)",
      textTransform: "uppercase",
      letterSpacing: ".06em"
    }
  }, it.lbl), /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 18,
      fontWeight: 700,
      color: it.color,
      marginTop: 1,
      fontVariantNumeric: "tabular-nums"
    }
  }, moneyShort(it.val))))), /*#__PURE__*/React.createElement("div", {
    style: {
      width: 1,
      background: "var(--gray-line)",
      height: 44
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      display: "flex",
      alignItems: "center",
      gap: 12,
      padding: "10px 16px"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      background: "#EDE9FE",
      borderRadius: 10,
      padding: 10,
      display: "flex"
    }
  }, /*#__PURE__*/React.createElement(MIcon, {
    name: "account_balance_wallet",
    size: 20,
    color: "#8B5CF6"
  })), /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 10,
      fontWeight: 600,
      color: "var(--gray-muted)",
      textTransform: "uppercase",
      letterSpacing: ".06em"
    }
  }, "Total caja"), /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 22,
      fontWeight: 700,
      color: "#8B5CF6",
      marginTop: 1,
      fontVariantNumeric: "tabular-nums"
    }
  }, moneyShort(total)))));
}
function DashboardView({
  isLoading,
  onRefresh
}) {
  return /*#__PURE__*/React.createElement(React.Fragment, null, /*#__PURE__*/React.createElement(ContentHeader, {
    title: "Resumen del D\xEDa",
    sub: "jueves, 21 de mayo de 2026",
    right: /*#__PURE__*/React.createElement(React.Fragment, null, isLoading && /*#__PURE__*/React.createElement("span", {
      style: {
        width: 22,
        height: 22,
        border: "3px solid rgba(139,92,246,.2)",
        borderTopColor: "#8B5CF6",
        borderRadius: "50%",
        animation: "spin .8s linear infinite",
        display: "inline-block"
      }
    }), /*#__PURE__*/React.createElement("button", {
      className: "btn-refresh",
      onClick: onRefresh
    }, /*#__PURE__*/React.createElement(MIcon, {
      name: "refresh"
    }), "Actualizar"))
  }), /*#__PURE__*/React.createElement("div", {
    className: "main-scroll"
  }, /*#__PURE__*/React.createElement("div", {
    style: dashStyles.root
  }, /*#__PURE__*/React.createElement("div", {
    style: dashStyles.kpiRow
  }, KPI_DATA.map((k, i) => /*#__PURE__*/React.createElement(KpiCard, {
    key: i,
    data: k
  }))), /*#__PURE__*/React.createElement("div", {
    style: dashStyles.tabRow
  }, TABLES.map((t, i) => /*#__PURE__*/React.createElement(TableCard, _extends({
    key: i
  }, t)))), /*#__PURE__*/React.createElement("div", {
    style: dashStyles.cajaWrap
  }, /*#__PURE__*/React.createElement(CajaFooter, null)))));
}
Object.assign(window, {
  DashboardView
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/shendevour-web/components/Dashboard.jsx", error: String((e && e.message) || e) }); }

// ui_kits/shendevour-web/components/Reproceso.jsx
try { (() => {
/* eslint-disable */
// ─────────────────────────────────────────────────────────────
// Reproceso view — clasificación de notas fiscales / no fiscales
// ─────────────────────────────────────────────────────────────

const PAY_MAP = {
  efec: {
    kind: "efec",
    icon: "payments",
    label: "Efectivo"
  },
  tcred: {
    kind: "tcred",
    icon: "credit_card",
    label: "T. Crédito"
  },
  tdeb: {
    kind: "tdeb",
    icon: "credit_card",
    label: "T. Débito"
  },
  trans: {
    kind: "trans",
    icon: "swap_horiz",
    label: "Transferencia"
  }
};
const NOTAS_SEED = [{
  num: 4218,
  origen: "Recepción · M.",
  cliente: "García López, Luis Antonio",
  total: 4820,
  pago: "tcred",
  facturada: true,
  movs: [{
    tipo: "cargo",
    concepto: "Hospedaje 1 noche",
    grupo: "Renta · Hab 318",
    ref: "Folio 4218-A",
    monto: 4200
  }, {
    tipo: "cargo",
    concepto: "Servicio a habitación",
    grupo: "Restaurante",
    ref: "Tic-1820",
    monto: 1240
  }, {
    tipo: "abono",
    concepto: "Depósito reserva",
    grupo: "Pago anticipado",
    ref: "Trans-918",
    monto: -620
  }]
}, {
  num: 4219,
  origen: "Recepción · M.",
  cliente: "Hernández Vega, María José",
  total: 1950,
  pago: "efec",
  facturada: false,
  movs: [{
    tipo: "cargo",
    concepto: "Hospedaje 1 noche",
    grupo: "Renta · Hab 404",
    ref: "Folio 4219-A",
    monto: 1950
  }]
}, {
  num: 4220,
  origen: "Restaurante",
  cliente: "Venta al público",
  total: 620,
  pago: "efec",
  facturada: false,
  movs: [{
    tipo: "cargo",
    concepto: "Desayuno buffet x2",
    grupo: "Restaurante",
    ref: "Tic-1821",
    monto: 620
  }]
}, {
  num: 4221,
  origen: "Recepción · M.",
  cliente: "Méndez Torres, Roberto",
  total: 2960,
  pago: "tdeb",
  facturada: false,
  movs: [{
    tipo: "cargo",
    concepto: "Hospedaje 1 noche",
    grupo: "Renta · Hab 206",
    ref: "Folio 4221-A",
    monto: 2100
  }, {
    tipo: "cargo",
    concepto: "Lavandería",
    grupo: "Servicios",
    ref: "OS-220",
    monto: 860
  }]
}, {
  num: 4222,
  origen: "Recepción · M.",
  cliente: "Robles Castro, Ana Cristina",
  total: 1240,
  pago: "trans",
  facturada: true,
  movs: [{
    tipo: "cargo",
    concepto: "Servicio a habitación",
    grupo: "Restaurante",
    ref: "Tic-1822",
    monto: 1240
  }]
}, {
  num: 4223,
  origen: "Restaurante",
  cliente: "Venta al público",
  total: 840,
  pago: "efec",
  facturada: false,
  movs: [{
    tipo: "cargo",
    concepto: "Comida x2",
    grupo: "Restaurante",
    ref: "Tic-1823",
    monto: 840
  }]
}];
function NotaRow({
  nota,
  selected,
  expanded,
  onSelect,
  onExpand
}) {
  const pay = PAY_MAP[nota.pago];
  return /*#__PURE__*/React.createElement("div", {
    style: {
      background: "#fff",
      borderRadius: 8,
      boxShadow: "var(--shadow)",
      marginBottom: 6,
      overflow: "hidden"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "grid",
      gridTemplateColumns: "24px 70px 140px 1fr 120px 240px 80px",
      alignItems: "center",
      gap: 12,
      padding: "10px 14px"
    }
  }, /*#__PURE__*/React.createElement(Checkbox, {
    checked: selected,
    disabled: nota.facturada,
    onChange: onSelect
  }), /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 9,
      fontWeight: 700,
      color: "var(--gray-muted)",
      letterSpacing: ".08em"
    }
  }, "NOTA"), /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 14,
      fontWeight: 700,
      color: "var(--gray-dark)"
    }
  }, nota.num)), /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 9,
      fontWeight: 700,
      color: "var(--gray-muted)",
      letterSpacing: ".08em"
    }
  }, "ORIGEN"), /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 11,
      color: "var(--gray-700)"
    }
  }, nota.origen)), /*#__PURE__*/React.createElement("div", {
    style: {
      minWidth: 0
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 9,
      fontWeight: 700,
      color: "var(--gray-muted)",
      letterSpacing: ".08em"
    }
  }, "CLIENTE"), /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 13,
      color: "var(--gray-dark)",
      overflow: "hidden",
      textOverflow: "ellipsis",
      whiteSpace: "nowrap"
    }
  }, nota.cliente)), /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 9,
      fontWeight: 700,
      color: "var(--gray-muted)",
      letterSpacing: ".08em"
    }
  }, "TOTAL NETO"), /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 15,
      fontWeight: 700,
      color: "var(--primary)",
      fontVariantNumeric: "tabular-nums"
    }
  }, money(nota.total))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      gap: 6,
      justifyContent: "flex-end"
    }
  }, /*#__PURE__*/React.createElement(Chip, {
    kind: pay.kind,
    icon: pay.icon
  }, pay.label), nota.facturada && /*#__PURE__*/React.createElement(Chip, {
    kind: "fact",
    icon: "check_circle"
  }, "Facturada")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      gap: 4,
      color: "var(--gray-muted)",
      fontSize: 12,
      justifyContent: "flex-end",
      cursor: "pointer"
    },
    onClick: onExpand
  }, nota.movs.length, " movs", /*#__PURE__*/React.createElement(MIcon, {
    name: expanded ? "expand_less" : "expand_more",
    size: 18
  }))), expanded && /*#__PURE__*/React.createElement("div", {
    style: {
      background: "#e9e9e9",
      padding: "10px 14px",
      borderTop: "1px solid var(--gray-line)"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      gap: 10,
      marginBottom: 6
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 9,
      fontWeight: 700,
      color: "var(--gray-muted)",
      letterSpacing: ".08em"
    }
  }, "MOVIMIENTOS"), /*#__PURE__*/React.createElement("span", {
    style: {
      padding: "2px 8px",
      borderRadius: 8,
      background: "#fff",
      fontSize: 11,
      fontWeight: 600,
      color: "#1565C0"
    }
  }, "Cargos: ", money(nota.movs.filter(m => m.tipo === "cargo").reduce((a, m) => a + m.monto, 0))), /*#__PURE__*/React.createElement("span", {
    style: {
      padding: "2px 8px",
      borderRadius: 8,
      background: "#fff",
      fontSize: 11,
      fontWeight: 600,
      color: "#2E7D32"
    }
  }, "Abonos: ", money(Math.abs(nota.movs.filter(m => m.tipo === "abono").reduce((a, m) => a + m.monto, 0))))), nota.movs.map((m, i) => /*#__PURE__*/React.createElement("div", {
    key: i,
    style: {
      display: "grid",
      gridTemplateColumns: "90px 1fr 120px 110px",
      alignItems: "center",
      gap: 10,
      padding: "5px 0",
      borderBottom: i < nota.movs.length - 1 ? ".5px solid #d8d8d8" : "none",
      fontSize: 12
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      padding: "2px 8px",
      borderRadius: 6,
      fontSize: 10,
      fontWeight: 700,
      textAlign: "center",
      textTransform: "uppercase",
      letterSpacing: ".04em",
      background: m.tipo === "cargo" ? "rgba(33,150,243,.18)" : "rgba(76,175,80,.18)",
      color: m.tipo === "cargo" ? "#1565C0" : "#2E7D32"
    }
  }, m.tipo), /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("div", null, m.concepto), /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 10,
      color: "var(--gray-muted)"
    }
  }, m.grupo)), /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 11,
      color: "var(--gray-muted)"
    }
  }, m.ref), /*#__PURE__*/React.createElement("span", {
    style: {
      fontWeight: 700,
      textAlign: "right",
      fontVariantNumeric: "tabular-nums",
      color: m.tipo === "cargo" ? "#1565C0" : "#2E7D32"
    }
  }, money(m.monto))))));
}
function ReprocesoView({
  isLoading,
  onRefresh
}) {
  const [notas, setNotas] = React.useState(() => NOTAS_SEED.map(n => ({
    ...n,
    selected: !n.facturada && n.pago !== "efec",
    expanded: false
  })));
  const [orden, setOrden] = React.useState({
    campo: "num",
    asc: true
  });
  const toggleSelect = i => setNotas(notas.map((n, j) => j === i ? {
    ...n,
    selected: !n.selected
  } : n));
  const toggleExpand = i => setNotas(notas.map((n, j) => j === i ? {
    ...n,
    expanded: !n.expanded
  } : n));
  const selectAll = () => setNotas(notas.map(n => ({
    ...n,
    selected: !n.facturada
  })));
  const deselectAll = () => setNotas(notas.map(n => ({
    ...n,
    selected: false
  })));
  const setSort = campo => setOrden(o => o.campo === campo ? {
    campo,
    asc: !o.asc
  } : {
    campo,
    asc: true
  });
  const kpis = React.useMemo(() => {
    const total = notas.length;
    const sel = notas.filter(n => n.selected && !n.facturada).length;
    const fact = notas.filter(n => n.facturada).length;
    const unf = total - fact;
    const tot = notas.reduce((a, n) => a + n.total, 0);
    return {
      total,
      sel,
      fact,
      unf,
      tot
    };
  }, [notas]);
  const footers = React.useMemo(() => {
    const a = notas.filter(n => n.selected && !n.facturada).reduce((a, n) => a + n.total, 0);
    const b = notas.filter(n => !n.selected && !n.facturada).reduce((a, n) => a + n.total, 0);
    const c = notas.filter(n => n.facturada).reduce((a, n) => a + n.total, 0);
    return {
      a,
      b,
      c
    };
  }, [notas]);
  return /*#__PURE__*/React.createElement(React.Fragment, null, /*#__PURE__*/React.createElement(ContentHeader, {
    title: "REPROCESO",
    sub: "clasificaci\xF3n de notas fiscales / no fiscales",
    right: /*#__PURE__*/React.createElement("button", {
      className: "btn-refresh",
      onClick: onRefresh
    }, /*#__PURE__*/React.createElement(MIcon, {
      name: "refresh"
    }), "Actualizar")
  }), /*#__PURE__*/React.createElement("div", {
    className: "main-scroll",
    style: {
      padding: 12,
      display: "flex",
      flexDirection: "column",
      gap: 8
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      background: "#fff",
      borderRadius: 10,
      boxShadow: "var(--shadow-2)",
      padding: "12px 16px",
      display: "flex",
      alignItems: "center",
      gap: 12,
      flexWrap: "wrap"
    }
  }, /*#__PURE__*/React.createElement(MIcon, {
    name: "calendar_today",
    size: 22,
    color: "var(--primary)"
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: "relative"
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      position: "absolute",
      top: -8,
      left: 12,
      background: "#fff",
      padding: "0 6px",
      fontSize: 10,
      color: "var(--primary)",
      fontWeight: 500
    }
  }, "Fecha inicio"), /*#__PURE__*/React.createElement("input", {
    style: {
      border: "1.5px solid var(--primary)",
      borderRadius: 4,
      padding: "8px 12px",
      fontFamily: "var(--font)",
      fontSize: 13,
      minWidth: 160
    },
    defaultValue: "21/05/2026"
  })), /*#__PURE__*/React.createElement("span", {
    style: {
      color: "var(--gray-muted)"
    }
  }, "\u2014"), /*#__PURE__*/React.createElement("div", {
    style: {
      position: "relative"
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      position: "absolute",
      top: -8,
      left: 12,
      background: "#fff",
      padding: "0 6px",
      fontSize: 10,
      color: "var(--primary)",
      fontWeight: 500
    }
  }, "Fecha fin"), /*#__PURE__*/React.createElement("input", {
    style: {
      border: "1.5px solid var(--primary)",
      borderRadius: 4,
      padding: "8px 12px",
      fontFamily: "var(--font)",
      fontSize: 13,
      minWidth: 160
    },
    defaultValue: "21/05/2026"
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      width: 1,
      background: "var(--gray-line)",
      height: 36
    }
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      flex: 1,
      fontSize: 12,
      fontStyle: "italic",
      color: "var(--gray-muted)"
    }
  }, kpis.total, " notas cargadas \xB7 ", kpis.sel, " seleccionadas para reproceso"), /*#__PURE__*/React.createElement("button", {
    className: "btn btn-outlined",
    onClick: selectAll
  }, /*#__PURE__*/React.createElement(MIcon, {
    name: "done_all"
  }), "Todas"), /*#__PURE__*/React.createElement("button", {
    className: "btn btn-outlined",
    onClick: deselectAll
  }, "Ninguna"), /*#__PURE__*/React.createElement("button", {
    className: "btn btn-raised",
    disabled: isLoading
  }, /*#__PURE__*/React.createElement(MIcon, {
    name: "search"
  }), "Buscar")), /*#__PURE__*/React.createElement("div", {
    style: {
      background: "#fff",
      borderRadius: 8,
      boxShadow: "var(--shadow)",
      padding: "7px 12px",
      display: "flex",
      alignItems: "center",
      gap: 4
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 11,
      color: "var(--gray-muted)",
      display: "flex",
      alignItems: "center",
      gap: 6,
      marginRight: 8
    }
  }, /*#__PURE__*/React.createElement(MIcon, {
    name: "sort",
    size: 16
  }), "Ordenar por:"), [{
    k: "num",
    l: "# Nota"
  }, {
    k: "cliente",
    l: "Cliente"
  }, {
    k: "pago",
    l: "Forma de pago"
  }, {
    k: "total",
    l: "Total"
  }, {
    k: "facturada",
    l: "Facturadas"
  }, {
    k: "origen",
    l: "Origen"
  }].map(s => /*#__PURE__*/React.createElement("button", {
    key: s.k,
    className: "btn btn-flat" + (orden.campo === s.k ? " active" : ""),
    onClick: () => setSort(s.k)
  }, s.l, /*#__PURE__*/React.createElement(MIcon, {
    name: orden.campo === s.k ? orden.asc ? "north" : "south" : "unfold_more",
    size: 14,
    color: orden.campo === s.k ? "var(--primary)" : "var(--gray-muted)"
  })))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "grid",
      gridTemplateColumns: "repeat(5, 1fr)",
      gap: 8
    }
  }, [{
    lbl: "TOTAL NOTAS",
    val: kpis.total,
    color: "var(--primary)",
    icon: "receipt"
  }, {
    lbl: "SELECCIONADAS",
    val: kpis.sel,
    color: "#9C27B0",
    icon: "check_box"
  }, {
    lbl: "FACTURADAS",
    val: kpis.fact,
    color: "#4CAF50",
    icon: "check_circle"
  }, {
    lbl: "SIN FACTURA",
    val: kpis.unf,
    color: "#FF9800",
    icon: "error_outline"
  }, {
    lbl: "TOTAL GENERAL",
    val: money(kpis.tot),
    color: "#323232",
    icon: "payments",
    isMoney: true
  }].map((k, i) => /*#__PURE__*/React.createElement("div", {
    key: i,
    style: {
      background: "#fff",
      borderRadius: 8,
      boxShadow: "var(--shadow)",
      padding: 14
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      gap: 6,
      marginBottom: 4
    }
  }, /*#__PURE__*/React.createElement(MIcon, {
    name: k.icon,
    size: 16,
    color: k.color
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 9,
      fontWeight: 700,
      color: "var(--gray-muted)",
      letterSpacing: ".08em"
    }
  }, k.lbl)), /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: k.isMoney ? 22 : 28,
      fontWeight: 700,
      lineHeight: 1,
      marginTop: 4,
      fontFamily: "var(--font-display)",
      color: k.color,
      fontVariantNumeric: "tabular-nums"
    }
  }, k.val)))), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 4
    }
  }, notas.map((n, i) => /*#__PURE__*/React.createElement(NotaRow, {
    key: n.num,
    nota: n,
    selected: n.selected,
    expanded: n.expanded,
    onSelect: () => toggleSelect(i),
    onExpand: () => toggleExpand(i)
  })))), /*#__PURE__*/React.createElement("div", {
    style: {
      background: "#fff",
      borderTop: "1px solid var(--gray-line)",
      padding: "10px 16px",
      display: "flex",
      alignItems: "center",
      gap: 12
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      display: "flex",
      gap: 8
    }
  }, [{
    lbl: "A REPROCESAR",
    val: footers.a,
    color: "#2E7D32",
    bg: "#E8F5E9",
    border: "#A5D6A7",
    icon: "check_circle"
  }, {
    lbl: "VENTAS AL PÚBLICO",
    val: footers.b,
    color: "#BF360C",
    bg: "#FFF3E0",
    border: "#FFCC80",
    icon: "storefront"
  }, {
    lbl: "YA FACTURADAS",
    val: footers.c,
    color: "#0D47A1",
    bg: "#E3F2FD",
    border: "#90CAF9",
    icon: "receipt_long"
  }].map((k, i) => /*#__PURE__*/React.createElement("div", {
    key: i,
    style: {
      display: "flex",
      alignItems: "center",
      gap: 10,
      borderRadius: 8,
      padding: "10px 14px",
      background: k.bg,
      border: "1px solid " + k.border
    }
  }, /*#__PURE__*/React.createElement(MIcon, {
    name: k.icon,
    size: 20,
    color: k.color
  }), /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 9,
      fontWeight: 700,
      letterSpacing: ".08em",
      color: "var(--gray-muted)"
    }
  }, k.lbl), /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 16,
      fontWeight: 700,
      color: k.color,
      fontVariantNumeric: "tabular-nums",
      marginTop: 1
    }
  }, money(k.val)))))), /*#__PURE__*/React.createElement("button", {
    className: "btn btn-danger",
    disabled: isLoading || kpis.sel === 0
  }, /*#__PURE__*/React.createElement(MIcon, {
    name: "refresh"
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      display: "flex",
      flexDirection: "column",
      lineHeight: 1.1,
      alignItems: "flex-start",
      marginLeft: 4
    }
  }, /*#__PURE__*/React.createElement("span", null, "Reprocesar notas"), /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 9,
      fontWeight: 500,
      opacity: .9,
      textTransform: "none",
      letterSpacing: 0
    }
  }, kpis.sel, " seleccionada(s)")))));
}
Object.assign(window, {
  ReprocesoView
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/shendevour-web/components/Reproceso.jsx", error: String((e && e.message) || e) }); }

// ui_kits/shendevour-web/components/Restaurant.jsx
try { (() => {
/* eslint-disable */
// ─────────────────────────────────────────────────────────────
// Restaurant view — rack de mesas + acciones rápidas + dock de pedidos
// ─────────────────────────────────────────────────────────────

const R_STATES = {
  available: {
    label: "Libre",
    border: "#D1D5DB",
    bg: "#FFFFFF",
    fg: "#6B7280",
    chairBg: "#E5E7EB"
  },
  occupied: {
    label: "Ocupada",
    border: "#6366F1",
    bg: "#EEF2FF",
    fg: "#4338CA",
    chairBg: "#A5B4FC"
  },
  reserved: {
    label: "Reservada",
    border: "#F59E0B",
    bg: "#FEF3C7",
    fg: "#92400E",
    chairBg: "#FBBF24"
  },
  cleaning: {
    label: "Limpiando",
    border: "#8B5CF6",
    bg: "#EDE9FE",
    fg: "#6D28D9",
    chairBg: "#C4B5FD"
  },
  attention: {
    label: "Atención",
    border: "#F97316",
    bg: "#FFEDD5",
    fg: "#9A3412",
    chairBg: "#FB923C"
  }
};
const R_TABLES = [
// Salón principal — frente
{
  id: "01",
  x: 20,
  y: 16,
  w: 88,
  h: 88,
  shape: "round",
  seats: 4,
  state: "occupied",
  guests: 3,
  since: 42,
  server: "María"
}, {
  id: "02",
  x: 140,
  y: 16,
  w: 88,
  h: 88,
  shape: "round",
  seats: 4,
  state: "available"
}, {
  id: "03",
  x: 260,
  y: 16,
  w: 88,
  h: 88,
  shape: "round",
  seats: 4,
  state: "reserved",
  resBy: "Soto",
  resAt: "14:30"
}, {
  id: "04",
  x: 380,
  y: 16,
  w: 88,
  h: 88,
  shape: "round",
  seats: 2,
  state: "occupied",
  guests: 2,
  since: 18,
  server: "Luis"
}, {
  id: "05",
  x: 500,
  y: 16,
  w: 88,
  h: 88,
  shape: "round",
  seats: 4,
  state: "available"
},
// Salón principal — mesas largas
{
  id: "06",
  x: 20,
  y: 136,
  w: 200,
  h: 76,
  shape: "rect",
  seats: 6,
  state: "attention",
  guests: 6,
  since: 85,
  server: "Pedro"
}, {
  id: "07",
  x: 250,
  y: 136,
  w: 200,
  h: 76,
  shape: "rect",
  seats: 6,
  state: "occupied",
  guests: 4,
  since: 30,
  server: "María"
}, {
  id: "08",
  x: 480,
  y: 136,
  w: 108,
  h: 76,
  shape: "rect",
  seats: 4,
  state: "cleaning"
},
// Terraza
{
  id: "T1",
  x: 20,
  y: 264,
  w: 88,
  h: 88,
  shape: "round",
  seats: 4,
  state: "occupied",
  guests: 4,
  since: 12,
  server: "Luis"
}, {
  id: "T2",
  x: 140,
  y: 264,
  w: 88,
  h: 88,
  shape: "round",
  seats: 4,
  state: "available"
}, {
  id: "T3",
  x: 260,
  y: 264,
  w: 88,
  h: 88,
  shape: "round",
  seats: 4,
  state: "reserved",
  resBy: "Vega",
  resAt: "15:00"
}, {
  id: "T4",
  x: 380,
  y: 264,
  w: 88,
  h: 88,
  shape: "round",
  seats: 4,
  state: "available"
}, {
  id: "T5",
  x: 500,
  y: 264,
  w: 88,
  h: 88,
  shape: "round",
  seats: 4,
  state: "occupied",
  guests: 2,
  since: 8,
  server: "Ana"
}];
const R_ZONES = [{
  label: "Salón principal",
  y: 0,
  h: 220
}, {
  label: "Terraza",
  y: 240,
  h: 130
}];

// Chair positions around a table (relative offsets from table edges)
function restChairs(table) {
  const cs = [];
  if (table.shape === "round") {
    // 4 chairs at N, S, E, W
    cs.push({
      side: "top",
      left: "50%",
      top: "-8px",
      tx: -8,
      ty: 0
    });
    cs.push({
      side: "bottom",
      left: "50%",
      top: "calc(100% - 8px)",
      tx: -8,
      ty: 0
    });
    cs.push({
      side: "left",
      left: "-8px",
      top: "50%",
      tx: 0,
      ty: -8
    });
    cs.push({
      side: "right",
      left: "calc(100% - 8px)",
      top: "50%",
      tx: 0,
      ty: -8
    });
  } else {
    // 3 chairs top, 3 bottom (for 6-seat) or 2+2 for smaller
    const n = Math.max(2, Math.min(4, Math.floor(table.seats / 2)));
    for (let i = 0; i < n; i++) {
      const left = `${(i + 1) / (n + 1) * 100}%`;
      cs.push({
        side: "top",
        left,
        top: "-8px",
        tx: -8,
        ty: 0
      });
      cs.push({
        side: "bottom",
        left,
        top: "calc(100% - 8px)",
        tx: -8,
        ty: 0
      });
    }
  }
  return cs;
}
function RestFloorTable({
  table,
  selected,
  onSelect
}) {
  const s = R_STATES[table.state];
  const isRound = table.shape === "round";
  return /*#__PURE__*/React.createElement("div", {
    onClick: () => onSelect(table.id),
    style: {
      position: "absolute",
      left: table.x,
      top: table.y,
      width: table.w,
      height: table.h,
      cursor: "pointer",
      transition: "transform 120ms ease"
    }
  }, restChairs(table).map((c, i) => /*#__PURE__*/React.createElement("div", {
    key: i,
    style: {
      position: "absolute",
      left: c.left,
      top: c.top,
      width: 16,
      height: 16,
      borderRadius: "50%",
      background: s.chairBg,
      transform: `translate(${c.tx}px, ${c.ty}px)`,
      zIndex: 0,
      transition: "background 120ms ease"
    }
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      position: "absolute",
      inset: 0,
      background: s.bg,
      border: `2px solid ${s.border}`,
      borderRadius: isRound ? "50%" : 12,
      display: "flex",
      flexDirection: "column",
      alignItems: "center",
      justifyContent: "center",
      boxShadow: selected ? "0 0 0 3px rgba(99,102,241,.25), var(--shadow)" : "var(--shadow)",
      zIndex: 1,
      gap: 2
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: isRound ? 22 : 20,
      fontWeight: 800,
      color: s.fg,
      lineHeight: 1,
      fontVariantNumeric: "tabular-nums"
    }
  }, table.id), table.state === "occupied" && /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 10,
      fontWeight: 600,
      color: s.fg,
      opacity: .85,
      display: "flex",
      alignItems: "center",
      gap: 3
    }
  }, /*#__PURE__*/React.createElement(MIcon, {
    name: "group",
    size: 11,
    color: s.fg
  }), table.guests, /*#__PURE__*/React.createElement("span", {
    style: {
      opacity: .5,
      margin: "0 1px"
    }
  }, "\xB7"), /*#__PURE__*/React.createElement(MIcon, {
    name: "schedule",
    size: 11,
    color: s.fg
  }), table.since, "m"), table.state === "reserved" && /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 10,
      fontWeight: 600,
      color: s.fg,
      textAlign: "center",
      lineHeight: 1.2
    }
  }, table.resBy, /*#__PURE__*/React.createElement("br", null), table.resAt), table.state === "attention" && /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 10,
      fontWeight: 700,
      color: s.fg,
      textTransform: "uppercase",
      letterSpacing: ".04em"
    }
  }, /*#__PURE__*/React.createElement(MIcon, {
    name: "priority_high",
    size: 12,
    color: s.fg,
    fill: true
  })), table.state === "cleaning" && /*#__PURE__*/React.createElement(MIcon, {
    name: "mop",
    size: 16,
    color: s.fg
  })));
}

// Quick-order cards (dock)
const R_ORDERS = [{
  mesa: "01",
  client: "Garcia, L.",
  party: 3,
  items: 5,
  total: 640,
  since: 12,
  status: "kitchen",
  server: "María"
}, {
  mesa: "04",
  client: "Pareja",
  party: 2,
  items: 3,
  total: 420,
  since: 8,
  status: "served",
  server: "Luis"
}, {
  mesa: "06",
  client: "Familia Pérez",
  party: 6,
  items: 9,
  total: 1850,
  since: 25,
  status: "bill",
  server: "Pedro"
}, {
  mesa: "07",
  client: "Hernández",
  party: 4,
  items: 6,
  total: 980,
  since: 18,
  status: "kitchen",
  server: "María"
}, {
  mesa: "T1",
  client: "Walk-in",
  party: 4,
  items: 4,
  total: 560,
  since: 6,
  status: "draft",
  server: "Luis"
}, {
  mesa: "T5",
  client: "Reyes, P.",
  party: 2,
  items: 2,
  total: 280,
  since: 4,
  status: "draft",
  server: "Ana"
}, {
  mesa: "BAR",
  client: "Barra · 3",
  party: 3,
  items: 5,
  total: 340,
  since: 14,
  status: "served",
  server: "Ana"
}, {
  mesa: "TO-GO",
  client: "Mtz. (recoge)",
  party: 1,
  items: 4,
  total: 480,
  since: 22,
  status: "ready",
  server: "—"
}];
const R_ORDER_STATUS = {
  draft: {
    label: "Borrador",
    bg: "#F3F4F6",
    fg: "#374151",
    icon: "edit_note"
  },
  kitchen: {
    label: "En cocina",
    bg: "#FEF3C7",
    fg: "#92400E",
    icon: "soup_kitchen"
  },
  ready: {
    label: "Listo",
    bg: "#DCFCE7",
    fg: "#065F46",
    icon: "room_service"
  },
  served: {
    label: "Servido",
    bg: "#E0E7FF",
    fg: "#4338CA",
    icon: "restaurant"
  },
  bill: {
    label: "Cuenta",
    bg: "#EDE9FE",
    fg: "#6D28D9",
    icon: "receipt_long"
  }
};
function RestOrderCard({
  order,
  active,
  onClick
}) {
  const st = R_ORDER_STATUS[order.status];
  return /*#__PURE__*/React.createElement("div", {
    onClick: onClick,
    style: {
      flex: "0 0 240px",
      background: "#fff",
      borderRadius: 12,
      padding: "10px 12px",
      boxShadow: active ? "0 0 0 2px var(--in), var(--shadow)" : "var(--shadow)",
      cursor: "pointer",
      display: "flex",
      flexDirection: "column",
      gap: 6,
      transition: "box-shadow 120ms ease"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      gap: 8
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      background: "#1C1E26",
      color: "#fff",
      padding: "3px 8px",
      borderRadius: 6,
      fontSize: 11,
      fontWeight: 800,
      letterSpacing: ".04em",
      fontVariantNumeric: "tabular-nums"
    }
  }, order.mesa), /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 13,
      fontWeight: 600,
      color: "var(--gray-dark)",
      overflow: "hidden",
      textOverflow: "ellipsis",
      whiteSpace: "nowrap",
      flex: 1
    }
  }, order.client), /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 10,
      color: "var(--gray-muted)",
      display: "flex",
      alignItems: "center",
      gap: 2
    }
  }, /*#__PURE__*/React.createElement(MIcon, {
    name: "group",
    size: 11
  }), order.party)), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      gap: 8,
      fontSize: 11.5,
      color: "var(--gray-muted)"
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      display: "flex",
      alignItems: "center",
      gap: 3
    }
  }, /*#__PURE__*/React.createElement(MIcon, {
    name: "restaurant_menu",
    size: 13
  }), order.items, " \xEDtems"), /*#__PURE__*/React.createElement("span", {
    style: {
      display: "flex",
      alignItems: "center",
      gap: 3
    }
  }, /*#__PURE__*/React.createElement(MIcon, {
    name: "schedule",
    size: 13
  }), order.since, "m"), /*#__PURE__*/React.createElement("span", {
    style: {
      marginLeft: "auto",
      color: "var(--gray-dark)",
      fontWeight: 700,
      fontVariantNumeric: "tabular-nums"
    }
  }, money(order.total))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      justifyContent: "space-between"
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      display: "inline-flex",
      alignItems: "center",
      gap: 4,
      background: st.bg,
      color: st.fg,
      padding: "3px 8px",
      borderRadius: 999,
      fontSize: 10.5,
      fontWeight: 700
    }
  }, /*#__PURE__*/React.createElement(MIcon, {
    name: st.icon,
    size: 13,
    color: st.fg
  }), st.label), /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 10.5,
      color: "var(--gray-muted)"
    }
  }, order.server)));
}

// Mini-KPI strip card
function RestMiniMetric({
  icon,
  label,
  value,
  subValue,
  color,
  bg
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      background: "#fff",
      borderRadius: 14,
      padding: "12px 16px",
      boxShadow: "var(--shadow)",
      display: "flex",
      alignItems: "center",
      gap: 12,
      minWidth: 0,
      flex: 1
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      width: 38,
      height: 38,
      borderRadius: 10,
      background: bg,
      display: "flex",
      alignItems: "center",
      justifyContent: "center",
      flexShrink: 0
    }
  }, /*#__PURE__*/React.createElement(MIcon, {
    name: icon,
    size: 20,
    color: color,
    fill: true
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      minWidth: 0,
      flex: 1
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 10,
      fontWeight: 700,
      color: "var(--gray-muted)",
      textTransform: "uppercase",
      letterSpacing: ".08em"
    }
  }, label), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "baseline",
      gap: 6,
      marginTop: 2
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 22,
      fontWeight: 700,
      color: color,
      lineHeight: 1,
      fontVariantNumeric: "tabular-nums"
    }
  }, value), subValue && /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 11,
      color: "var(--gray-muted)"
    }
  }, subValue))));
}
function RestaurantView({
  isLoading,
  onRefresh
}) {
  const [selected, setSelected] = React.useState(null);
  const [activeOrder, setActiveOrder] = React.useState(0);
  const [view, setView] = React.useState("mesa"); // mesa | lista

  const occCount = R_TABLES.filter(t => t.state === "occupied" || t.state === "attention").length;
  const totalTables = R_TABLES.length;
  const activeOrders = R_ORDERS.length;
  const dayRevenue = R_ORDERS.reduce((a, o) => a + o.total, 0) + 12480;
  const avgTicket = Math.round(dayRevenue / (occCount + 4));
  return /*#__PURE__*/React.createElement(React.Fragment, null, /*#__PURE__*/React.createElement(ContentHeader, {
    title: "Restaurante",
    sub: "turno matutino \xB7 12 mesas \xB7 2 zonas",
    right: /*#__PURE__*/React.createElement(React.Fragment, null, isLoading && /*#__PURE__*/React.createElement("span", {
      style: {
        width: 22,
        height: 22,
        border: "3px solid rgba(99,102,241,.2)",
        borderTopColor: "#6366F1",
        borderRadius: "50%",
        animation: "spin .8s linear infinite",
        display: "inline-block"
      }
    }), /*#__PURE__*/React.createElement("button", {
      className: "btn-refresh",
      onClick: onRefresh
    }, /*#__PURE__*/React.createElement(MIcon, {
      name: "refresh"
    }), "Actualizar"))
  }), /*#__PURE__*/React.createElement("div", {
    className: "main-scroll",
    style: {
      padding: "12px 14px 8px",
      display: "flex",
      flexDirection: "column",
      gap: 10
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "grid",
      gridTemplateColumns: "repeat(4, 1fr)",
      gap: 10
    }
  }, /*#__PURE__*/React.createElement(RestMiniMetric, {
    icon: "point_of_sale",
    label: "Ventas hoy",
    value: money(dayRevenue),
    subValue: `avg ${money(avgTicket)}`,
    color: "#10B981",
    bg: "#DCFCE7"
  }), /*#__PURE__*/React.createElement(RestMiniMetric, {
    icon: "table_restaurant",
    label: "Mesas ocupadas",
    value: `${occCount} / ${totalTables}`,
    subValue: `${Math.round(occCount / totalTables * 100)}%`,
    color: "#6366F1",
    bg: "#E0E7FF"
  }), /*#__PURE__*/React.createElement(RestMiniMetric, {
    icon: "receipt",
    label: "Pedidos activos",
    value: activeOrders,
    subValue: "3 en cocina",
    color: "#F59E0B",
    bg: "#FEF3C7"
  }), /*#__PURE__*/React.createElement(RestMiniMetric, {
    icon: "trending_up",
    label: "Ticket promedio",
    value: money(avgTicket),
    subValue: "+8% vs ayer",
    color: "#8B5CF6",
    bg: "#EDE9FE"
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      background: "#fff",
      borderRadius: 14,
      padding: "10px 14px",
      boxShadow: "var(--shadow)",
      display: "flex",
      alignItems: "center",
      gap: 10
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      position: "relative",
      flex: "0 0 280px"
    }
  }, /*#__PURE__*/React.createElement(MIcon, {
    name: "search",
    size: 18,
    color: "var(--gray-muted)",
    style: {
      position: "absolute",
      left: 10,
      top: "50%",
      transform: "translateY(-50%)"
    }
  }), /*#__PURE__*/React.createElement("input", {
    placeholder: "Buscar mesa, mesero, cliente o platillo\u2026",
    style: {
      width: "100%",
      border: "1.5px solid var(--gray-line)",
      borderRadius: 10,
      padding: "8px 12px 8px 36px",
      fontFamily: "var(--font)",
      fontSize: 12.5,
      outline: "none"
    }
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      gap: 6,
      flex: 1
    }
  }, /*#__PURE__*/React.createElement("button", {
    className: "btn btn-raised"
  }, /*#__PURE__*/React.createElement(MIcon, {
    name: "add_circle",
    fill: true
  }), "Nuevo pedido"), /*#__PURE__*/React.createElement("button", {
    className: "btn btn-outlined"
  }, /*#__PURE__*/React.createElement(MIcon, {
    name: "event_available"
  }), "Reservar mesa"), /*#__PURE__*/React.createElement("button", {
    className: "btn btn-flat"
  }, /*#__PURE__*/React.createElement(MIcon, {
    name: "local_dining"
  }), "Combinar mesas"), /*#__PURE__*/React.createElement("button", {
    className: "btn btn-flat"
  }, /*#__PURE__*/React.createElement(MIcon, {
    name: "tune"
  }), "Filtros")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      background: "var(--gray-foot)",
      borderRadius: 10,
      padding: 3,
      gap: 2
    }
  }, ["mesa", "lista"].map(v => /*#__PURE__*/React.createElement("button", {
    key: v,
    onClick: () => setView(v),
    style: {
      border: "none",
      background: view === v ? "#fff" : "transparent",
      boxShadow: view === v ? "var(--shadow)" : "none",
      padding: "5px 10px",
      borderRadius: 7,
      cursor: "pointer",
      display: "flex",
      alignItems: "center",
      gap: 5,
      fontFamily: "var(--font)",
      fontSize: 12,
      fontWeight: view === v ? 600 : 500,
      color: view === v ? "var(--gray-dark)" : "var(--gray-muted)"
    }
  }, /*#__PURE__*/React.createElement(MIcon, {
    name: v === "mesa" ? "grid_view" : "list_alt",
    size: 15
  }), v === "mesa" ? "Mesas" : "Lista")))), /*#__PURE__*/React.createElement("div", {
    style: {
      background: "#fff",
      borderRadius: 14,
      padding: "12px 14px",
      boxShadow: "var(--shadow)",
      flex: 1,
      minHeight: 0,
      display: "flex",
      flexDirection: "column"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      gap: 10,
      marginBottom: 6,
      flexShrink: 0,
      flexWrap: "wrap"
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 10,
      fontWeight: 700,
      color: "var(--gray-muted)",
      textTransform: "uppercase",
      letterSpacing: ".08em"
    }
  }, "RACK DE MESAS"), /*#__PURE__*/React.createElement("span", {
    style: {
      flex: 1
    }
  }), Object.entries(R_STATES).map(([k, v]) => /*#__PURE__*/React.createElement("div", {
    key: k,
    style: {
      display: "flex",
      alignItems: "center",
      gap: 5,
      fontSize: 10.5,
      color: "var(--gray-muted)"
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      width: 10,
      height: 10,
      borderRadius: "50%",
      background: v.bg,
      border: `2px solid ${v.border}`
    }
  }), v.label))), /*#__PURE__*/React.createElement("div", {
    style: {
      position: "relative",
      flex: 1,
      minHeight: 380,
      background: "linear-gradient(180deg, #FAFBFC 0%, #F4F6FA 100%)",
      borderRadius: 10,
      padding: 8,
      overflow: "hidden"
    }
  }, R_ZONES.map(z => /*#__PURE__*/React.createElement("div", {
    key: z.label,
    style: {
      position: "absolute",
      left: 10,
      top: z.y + 8,
      fontSize: 9,
      fontWeight: 700,
      color: "var(--gray-muted)",
      textTransform: "uppercase",
      letterSpacing: ".1em",
      writingMode: "vertical-rl",
      transform: "rotate(180deg)",
      height: z.h,
      display: "flex",
      alignItems: "center"
    }
  }, z.label)), /*#__PURE__*/React.createElement("div", {
    style: {
      position: "absolute",
      left: 32,
      right: 16,
      top: 232,
      height: 1,
      borderTop: "1px dashed var(--gray-line)"
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: "absolute",
      left: 36,
      top: 8,
      right: 8,
      bottom: 8
    }
  }, R_TABLES.map(t => /*#__PURE__*/React.createElement(RestFloorTable, {
    key: t.id,
    table: t,
    selected: selected === t.id,
    onSelect: setSelected
  })))))), /*#__PURE__*/React.createElement("div", {
    style: {
      background: "#fff",
      borderTop: "1px solid var(--gray-line)",
      boxShadow: "0 -2px 8px rgba(0,0,0,.04)",
      padding: "8px 14px 10px",
      flexShrink: 0,
      display: "flex",
      flexDirection: "column",
      gap: 6
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      gap: 8,
      padding: "0 4px"
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 10,
      fontWeight: 700,
      color: "var(--gray-muted)",
      textTransform: "uppercase",
      letterSpacing: ".08em"
    }
  }, "Pedidos r\xE1pidos"), /*#__PURE__*/React.createElement("span", {
    style: {
      background: "#1C1E26",
      color: "#fff",
      borderRadius: 999,
      padding: "2px 8px",
      fontSize: 10,
      fontWeight: 700
    }
  }, R_ORDERS.length), /*#__PURE__*/React.createElement("span", {
    style: {
      flex: 1
    }
  }), /*#__PURE__*/React.createElement("button", {
    className: "btn btn-flat",
    style: {
      fontSize: 11,
      height: 26
    }
  }, /*#__PURE__*/React.createElement(MIcon, {
    name: "filter_list",
    size: 14
  }), "Por estado"), /*#__PURE__*/React.createElement("button", {
    className: "btn btn-flat",
    style: {
      fontSize: 11,
      height: 26
    }
  }, /*#__PURE__*/React.createElement(MIcon, {
    name: "visibility",
    size: 14
  }), "Ver todos")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      gap: 10,
      overflowX: "auto",
      paddingBottom: 4,
      paddingTop: 2,
      paddingLeft: 4,
      paddingRight: 4,
      scrollbarWidth: "thin"
    }
  }, R_ORDERS.map((o, i) => /*#__PURE__*/React.createElement(RestOrderCard, {
    key: i,
    order: o,
    active: activeOrder === i,
    onClick: () => setActiveOrder(i)
  })))));
}
Object.assign(window, {
  RestaurantView
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/shendevour-web/components/Restaurant.jsx", error: String((e && e.message) || e) }); }

// ui_kits/shendevour-web/components/Shell.jsx
try { (() => {
/* eslint-disable */
// ─────────────────────────────────────────────────────────────
// Shell — Titlebar, Sidebar, Statusbar
// ─────────────────────────────────────────────────────────────

function Titlebar({
  crumb,
  view
}) {
  return /*#__PURE__*/React.createElement("header", {
    className: "titlebar"
  }, /*#__PURE__*/React.createElement("div", {
    className: "tb-left"
  }, /*#__PURE__*/React.createElement("div", {
    className: "tb-logo"
  }, /*#__PURE__*/React.createElement("div", {
    className: "tb-logo-icon"
  }, /*#__PURE__*/React.createElement("img", {
    src: "assets/keorsoft-logo.png",
    alt: "Keorsoft"
  })), /*#__PURE__*/React.createElement("div", {
    className: "tb-logo-name"
  }, "SH", /*#__PURE__*/React.createElement("b", null, "Endevour"))), /*#__PURE__*/React.createElement("nav", {
    className: "tb-crumb"
  }, /*#__PURE__*/React.createElement("span", null, crumb || "Hotel Misión GDL"), /*#__PURE__*/React.createElement("span", {
    className: "sep"
  }, "\u203A"), /*#__PURE__*/React.createElement("span", {
    className: "cur"
  }, view))), /*#__PURE__*/React.createElement("div", {
    className: "tb-right"
  }, /*#__PURE__*/React.createElement("div", {
    className: "tb-chip"
  }, /*#__PURE__*/React.createElement("div", {
    className: "dot"
  }), "Turno Matutino"), /*#__PURE__*/React.createElement("div", {
    className: "tb-chip"
  }, /*#__PURE__*/React.createElement(MIcon, {
    name: "notifications_none"
  }), "3"), /*#__PURE__*/React.createElement("button", {
    className: "tb-icon-btn",
    title: "Modo oscuro"
  }, /*#__PURE__*/React.createElement(MIcon, {
    name: "dark_mode"
  })), /*#__PURE__*/React.createElement("button", {
    className: "tb-icon-btn",
    title: "Ayuda"
  }, /*#__PURE__*/React.createElement(MIcon, {
    name: "help_outline"
  })), /*#__PURE__*/React.createElement("div", {
    className: "tb-avatar"
  }, "JR")));
}
const NAV_GROUPS = [{
  items: [{
    id: "dashboard",
    icon: "dashboard",
    label: "Dashboard"
  }, {
    id: "metrics",
    icon: "bar_chart",
    label: "Métricas del Turno"
  }]
}, {
  name: "Operaciones",
  items: [{
    id: "rooms",
    icon: "bed",
    label: "Habitaciones"
  }, {
    id: "checkins",
    icon: "login",
    label: "Registros",
    badge: "8"
  }, {
    id: "reservations",
    icon: "event",
    label: "Reservas",
    badge: "3",
    badgeKind: "am"
  }, {
    id: "caja",
    icon: "account_balance_wallet",
    label: "Caja"
  }, {
    id: "restaurant",
    icon: "restaurant",
    label: "Restaurante"
  }]
}, {
  name: "Gerencial",
  items: [{
    id: "reproceso",
    icon: "refresh",
    label: "Reproceso"
  }, {
    id: "billing",
    icon: "receipt_long",
    label: "Facturación"
  }, {
    id: "reports",
    icon: "summarize",
    label: "Reportes"
  }]
}, {
  name: "Sistema",
  items: [{
    id: "settings",
    icon: "settings",
    label: "Configuración"
  }, {
    id: "users",
    icon: "manage_accounts",
    label: "Usuarios"
  }]
}];
function Sidebar({
  active,
  onNav
}) {
  return /*#__PURE__*/React.createElement("aside", {
    className: "sidebar"
  }, /*#__PURE__*/React.createElement("div", {
    className: "sb-scroll"
  }, NAV_GROUPS.map((g, gi) => /*#__PURE__*/React.createElement(React.Fragment, {
    key: gi
  }, gi > 0 && /*#__PURE__*/React.createElement("div", {
    className: "sb-sep"
  }), g.name && /*#__PURE__*/React.createElement("div", {
    className: "sb-group"
  }, g.name), g.items.map(it => /*#__PURE__*/React.createElement("div", {
    key: it.id,
    className: "sb-item" + (active === it.id ? " active" : ""),
    onClick: () => onNav && onNav(it.id)
  }, /*#__PURE__*/React.createElement(MIcon, {
    name: it.icon
  }), /*#__PURE__*/React.createElement("span", {
    className: "sb-item-name"
  }, it.label), it.badge && /*#__PURE__*/React.createElement("span", {
    className: "sb-badge " + (it.badgeKind || "")
  }, it.badge)))))), /*#__PURE__*/React.createElement("div", {
    className: "sb-foot"
  }, /*#__PURE__*/React.createElement("div", {
    className: "sb-user"
  }, /*#__PURE__*/React.createElement("div", {
    className: "sb-user-av"
  }, "JR"), /*#__PURE__*/React.createElement("div", {
    className: "sb-user-info"
  }, /*#__PURE__*/React.createElement("div", {
    className: "sb-user-name"
  }, "J. Rodr\xEDguez"), /*#__PURE__*/React.createElement("div", {
    className: "sb-user-role"
  }, "Recepcionista \xB7 Mat.")), /*#__PURE__*/React.createElement("button", {
    className: "tb-icon-btn",
    title: "Cerrar sesi\xF3n",
    style: {
      width: 28,
      height: 28
    }
  }, /*#__PURE__*/React.createElement(MIcon, {
    name: "logout",
    size: 16
  })))));
}
function Statusbar() {
  return /*#__PURE__*/React.createElement("div", {
    className: "statusbar"
  }, /*#__PURE__*/React.createElement("div", {
    className: "sb-l"
  }, /*#__PURE__*/React.createElement("div", {
    className: "sb-i"
  }, /*#__PURE__*/React.createElement("span", {
    className: "sb-status-dot"
  }), "Conectado"), /*#__PURE__*/React.createElement("div", {
    className: "sb-i"
  }, "Turno: Matutino (08:00 \u2013 16:00)"), /*#__PURE__*/React.createElement("div", {
    className: "sb-i"
  }, "Usuario: J. Rodr\xEDguez")), /*#__PURE__*/React.createElement("div", {
    className: "sb-r"
  }, /*#__PURE__*/React.createElement("div", {
    className: "sb-i"
  }, "Hotel Misi\xF3n GDL"), /*#__PURE__*/React.createElement("div", {
    className: "sb-i"
  }, "v2.4.1"), /*#__PURE__*/React.createElement("div", {
    className: "sb-i"
  }, "Jue 21 May 2026 \xB7 14:32")));
}
function ContentHeader({
  title,
  sub,
  right
}) {
  return /*#__PURE__*/React.createElement("div", {
    className: "content-header"
  }, /*#__PURE__*/React.createElement("div", {
    className: "ch-left"
  }, /*#__PURE__*/React.createElement("span", {
    className: "ch-title"
  }, title), sub && /*#__PURE__*/React.createElement("span", {
    className: "ch-sub"
  }, sub)), /*#__PURE__*/React.createElement("div", {
    className: "ch-right"
  }, right));
}
Object.assign(window, {
  Titlebar,
  Sidebar,
  Statusbar,
  ContentHeader
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/shendevour-web/components/Shell.jsx", error: String((e && e.message) || e) }); }

})();
