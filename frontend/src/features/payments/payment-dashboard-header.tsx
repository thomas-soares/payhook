import { Loader2, RefreshCw } from "lucide-react";

type PaymentDashboardHeaderProps = {
  isFetching: boolean;
  latestUpdate: string;
};

export function PaymentDashboardHeader({ isFetching, latestUpdate }: PaymentDashboardHeaderProps) {
  return (
    <header className="dashboard-header">
      <div>
        <p className="eyebrow">Payhook</p>
        <h1>Pagamentos recebidos</h1>
      </div>
      <div className="refresh-indicator" aria-live="polite">
        {isFetching ? (
          <Loader2 aria-hidden="true" className="spin" size={18} />
        ) : (
          <RefreshCw aria-hidden="true" size={18} />
        )}
        <span>Atualizado {latestUpdate}</span>
      </div>
    </header>
  );
}
