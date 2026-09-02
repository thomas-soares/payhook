type PaymentSummaryStripProps = {
  failedCount: number;
  pageCount: number;
  totalCount: number;
};

export function PaymentSummaryStrip({ failedCount, pageCount, totalCount }: PaymentSummaryStripProps) {
  return (
    <section className="summary-strip" aria-label="Resumo dos pagamentos">
      <SummaryMetric label="Total" value={totalCount} />
      <SummaryMetric label="Nesta pagina" value={pageCount} />
      <SummaryMetric label="Com erro" value={failedCount} intent={failedCount > 0 ? "danger" : "ok"} />
    </section>
  );
}

function SummaryMetric({
  intent,
  label,
  value
}: {
  intent?: "danger" | "ok";
  label: string;
  value: number;
}) {
  return (
    <div className={intent ? `summary-metric ${intent}` : "summary-metric"}>
      <span>{label}</span>
      <strong>{value}</strong>
    </div>
  );
}
