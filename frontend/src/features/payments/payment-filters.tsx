import { Search } from "lucide-react";
import type { PaymentFilters, PaymentStatusFilter } from "./types";

const statusOptions: Array<{ label: string; value: PaymentStatusFilter }> = [
  { label: "Todos", value: "all" },
  { label: "Sucesso", value: "Processed" },
  { label: "Erro", value: "Failed" }
];

type PaymentFiltersProps = {
  filters: PaymentFilters;
  onChange: (filters: PaymentFilters) => void;
};

export function PaymentFilters({ filters, onChange }: PaymentFiltersProps) {
  return (
    <section className="filters-panel" aria-label="Filtros">
      <div className="segmented-control" aria-label="Status">
        {statusOptions.map((option) => (
          <button
            aria-pressed={filters.status === option.value}
            className={filters.status === option.value ? "active" : ""}
            key={option.value}
            onClick={() => onChange({ ...filters, status: option.value })}
            type="button"
          >
            {option.label}
          </button>
        ))}
      </div>

      <label className="search-field">
        <Search aria-hidden="true" size={18} />
        <input
          onChange={(event) => onChange({ ...filters, contractId: event.target.value })}
          placeholder="ID do contrato"
          type="search"
          value={filters.contractId}
        />
      </label>
    </section>
  );
}
