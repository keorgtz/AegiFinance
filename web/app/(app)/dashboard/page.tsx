export default function DashboardPage() {
  return (
    <div>
      <div className="mb-6">
        <h1 className="font-display text-[22px] font-700 text-[#16181D]">Dashboard</h1>
        <p className="mt-1 text-[13px] text-[#5B6472]">
          Indicadores de negocio — Pendiente de implementar (Fase 10)
        </p>
      </div>

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
        {["Clientes Activos", "MRR", "Pendiente Cobro", "Ingresos Mes"].map((label) => (
          <div
            key={label}
            className="rounded-card bg-white shadow-dp1 p-5 flex flex-col gap-2"
          >
            <span className="text-[10px] font-600 uppercase tracking-wider text-[#5B6472]">
              {label}
            </span>
            <span className="font-display text-[28px] font-700 text-[#5B6472]">
              —
            </span>
          </div>
        ))}
      </div>
    </div>
  );
}
