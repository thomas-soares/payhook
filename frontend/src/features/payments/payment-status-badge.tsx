import { AlertTriangle, CheckCircle2, Loader2 } from "lucide-react";
import type { ProcessingStatus } from "./types";

const statusLabels: Record<ProcessingStatus, string> = {
  Pending: "Pendente",
  Processed: "Sucesso",
  Failed: "Erro"
};

type PaymentStatusBadgeProps = {
  status: ProcessingStatus;
};

export function PaymentStatusBadge({ status }: PaymentStatusBadgeProps) {
  const icon =
    status === "Failed" ? (
      <AlertTriangle aria-hidden="true" size={16} />
    ) : status === "Processed" ? (
      <CheckCircle2 aria-hidden="true" size={16} />
    ) : (
      <Loader2 aria-hidden="true" className="spin" size={16} />
    );

  return (
    <span className={`status-badge ${status.toLowerCase()}`}>
      {icon}
      {statusLabels[status]}
    </span>
  );
}
