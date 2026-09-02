"use client";

import { useQuery } from "@tanstack/react-query";
import { Loader2, X } from "lucide-react";
import { formatAmount, formatDate } from "@/lib/formatters";
import { fetchPaymentDetail } from "./api";

type PaymentErrorDetailPanelProps = {
  paymentId: string | null;
  onClose: () => void;
};

export function PaymentErrorDetailPanel({ onClose, paymentId }: PaymentErrorDetailPanelProps) {
  const detailQuery = useQuery({
    enabled: paymentId !== null,
    queryKey: ["payment-detail", paymentId],
    queryFn: () => fetchPaymentDetail(paymentId!)
  });

  if (paymentId === null) {
    return null;
  }

  return (
    <div className="detail-backdrop">
      <aside aria-label="Detalhe do erro" aria-modal="true" className="detail-panel" role="dialog">
        <header className="detail-header">
          <div>
            <p className="eyebrow">Erro</p>
            <h2>Detalhe do pagamento</h2>
          </div>
          <button aria-label="Fechar detalhe" className="icon-button" onClick={onClose} type="button">
            <X aria-hidden="true" size={20} />
          </button>
        </header>

        {detailQuery.isLoading ? (
          <div className="detail-state" role="status">
            <Loader2 aria-hidden="true" className="spin" size={22} />
            <span>Carregando detalhe...</span>
          </div>
        ) : null}

        {detailQuery.isError ? (
          <div className="detail-state error" role="alert">
            {detailQuery.error.message}
          </div>
        ) : null}

        {detailQuery.data ? (
          <div className="detail-content">
            <div className="detail-grid">
              <DetailItem label="Transacao" value={detailQuery.data.transactionId ?? "--"} />
              <DetailItem label="Contrato" value={detailQuery.data.contractId ?? "--"} />
              <DetailItem label="Valor" value={formatAmount(detailQuery.data.amount)} />
              <DetailItem label="Recebido em" value={formatDate(detailQuery.data.receivedAt)} />
            </div>

            <section className="detail-block">
              <h3>Motivo</h3>
              <p>{detailQuery.data.processingError ?? "Erro nao informado."}</p>
            </section>

            <section className="detail-block">
              <h3>Payload</h3>
              <pre>{formatPayload(detailQuery.data.payloadJson)}</pre>
            </section>
          </div>
        ) : null}
      </aside>
    </div>
  );
}

function DetailItem({ label, value }: { label: string; value: string }) {
  return (
    <div className="detail-item">
      <span>{label}</span>
      <strong>{value}</strong>
    </div>
  );
}

function formatPayload(payloadJson: string) {
  try {
    return JSON.stringify(JSON.parse(payloadJson), null, 2);
  } catch {
    return payloadJson;
  }
}
