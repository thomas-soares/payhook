import { TableState } from "@/components/table-state";
import { formatAmount, formatDate } from "@/lib/formatters";
import { PaymentStatusBadge } from "./payment-status-badge";
import type { PaymentSummary } from "./types";

type PaymentTableProps = {
  isLoading: boolean;
  onSelectError: (paymentId: string) => void;
  payments: PaymentSummary[];
  viewedErrorIds: Set<string>;
};

export function PaymentTable({ isLoading, onSelectError, payments, viewedErrorIds }: PaymentTableProps) {
  if (isLoading || payments.length === 0) {
    return (
      <TableState
        emptyLabel="Nenhum pagamento encontrado."
        isLoading={isLoading}
        loadingLabel="Carregando pagamentos..."
      />
    );
  }

  return (
    <div className="table-wrap">
      <table>
        <colgroup>
          <col className="status-column" />
          <col className="identifier-column" />
          <col className="identifier-column" />
          <col className="amount-column" />
          <col className="payment-column" />
          <col className="date-column" />
          <col className="detail-column" />
        </colgroup>
        <thead>
          <tr>
            <th>Status</th>
            <th>Transacao</th>
            <th>Contrato</th>
            <th>Valor</th>
            <th>Pagamento</th>
            <th>Recebido em</th>
            <th>Detalhe</th>
          </tr>
        </thead>
        <tbody>
          {payments.map((payment) => (
            <tr key={payment.id}>
              <td>
                <PaymentStatusBadge status={payment.processingStatus} />
              </td>
              <td className="mono">
                <CompactIdentifier value={payment.transactionId} />
              </td>
              <td className="mono">
                <CompactIdentifier value={payment.contractId} />
              </td>
              <td>{formatAmount(payment.amount)}</td>
              <td>{payment.paymentStatus ?? "--"}</td>
              <td>{formatDate(payment.receivedAt)}</td>
              <td>
                {payment.processingStatus === "Failed" ? (
                  <button
                    className={
                      viewedErrorIds.has(payment.id) ? "detail-button viewed" : "detail-button"
                    }
                    onClick={() => onSelectError(payment.id)}
                    type="button"
                  >
                    {viewedErrorIds.has(payment.id) ? "Visto" : "Ver erro"}
                  </button>
                ) : (
                  "--"
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function CompactIdentifier({ value }: { value: string | null }) {
  if (!value) {
    return "--";
  }

  return (
    <span className="compact-id" title={value}>
      {formatIdentifier(value)}
    </span>
  );
}

function formatIdentifier(value: string) {
  if (value.length <= 24) {
    return value;
  }

  return `${value.slice(0, 12)}...${value.slice(-8)}`;
}
