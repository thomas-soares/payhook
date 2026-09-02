import { TableState } from "@/components/table-state";
import { formatAmount, formatDate } from "@/lib/formatters";
import { PaymentStatusBadge } from "./payment-status-badge";
import type { PaymentSummary } from "./types";

type PaymentTableProps = {
  isLoading: boolean;
  payments: PaymentSummary[];
};

export function PaymentTable({ isLoading, payments }: PaymentTableProps) {
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
        <thead>
          <tr>
            <th>Status</th>
            <th>Transacao</th>
            <th>Contrato</th>
            <th>Valor</th>
            <th>Pagamento</th>
            <th>Recebido em</th>
          </tr>
        </thead>
        <tbody>
          {payments.map((payment) => (
            <tr key={payment.id}>
              <td>
                <PaymentStatusBadge status={payment.processingStatus} />
              </td>
              <td className="mono">{payment.transactionId}</td>
              <td className="mono">{payment.contractId}</td>
              <td>{formatAmount(payment.amount)}</td>
              <td>{payment.paymentStatus ?? "--"}</td>
              <td>{formatDate(payment.receivedAt)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
