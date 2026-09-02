"use client";

import { useQuery } from "@tanstack/react-query";
import { useMemo, useState } from "react";
import { formatTime } from "@/lib/formatters";
import { fetchPayments } from "./api";
import { PaymentDashboardHeader } from "./payment-dashboard-header";
import { PaymentErrorBanner } from "./payment-error-banner";
import { PaymentErrorDetailPanel } from "./payment-error-detail-panel";
import { PaymentFilters } from "./payment-filters";
import { PaymentSummaryStrip } from "./payment-summary-strip";
import { PaymentTable } from "./payment-table";
import { PaymentUnviewedErrorsAlert } from "./payment-unviewed-errors-alert";
import type { PaymentFilters as PaymentFiltersState } from "./types";

type PaymentDashboardProps = {
  pollIntervalMs?: number | false;
};

export function PaymentDashboard({ pollIntervalMs = 5000 }: PaymentDashboardProps) {
  const [filters, setFilters] = useState<PaymentFiltersState>({
    status: "all",
    contractId: ""
  });
  const [selectedErrorId, setSelectedErrorId] = useState<string | null>(null);
  const [viewedErrorIds, setViewedErrorIds] = useState(() => new Set<string>());

  const paymentsQuery = useQuery({
    queryKey: ["payments", filters],
    queryFn: () => fetchPayments(filters),
    refetchInterval: pollIntervalMs,
    refetchIntervalInBackground: true
  });

  const payments = useMemo(() => paymentsQuery.data?.items ?? [], [paymentsQuery.data?.items]);
  const failedCount = useMemo(
    () => payments.filter((payment) => payment.processingStatus === "Failed").length,
    [payments]
  );
  const unviewedFailedCount = useMemo(
    () =>
      payments.filter(
        (payment) => payment.processingStatus === "Failed" && !viewedErrorIds.has(payment.id)
      ).length,
    [payments, viewedErrorIds]
  );
  const latestUpdate = paymentsQuery.dataUpdatedAt ? formatTime(paymentsQuery.dataUpdatedAt) : "--";

  function handleSelectError(paymentId: string) {
    setSelectedErrorId(paymentId);
    setViewedErrorIds((current) => new Set(current).add(paymentId));
  }

  return (
    <main className="dashboard-shell">
      <PaymentDashboardHeader isFetching={paymentsQuery.isFetching} latestUpdate={latestUpdate} />

      {paymentsQuery.isError ? <PaymentErrorBanner message={paymentsQuery.error.message} /> : null}
      <PaymentUnviewedErrorsAlert count={unviewedFailedCount} />

      <PaymentSummaryStrip
        failedCount={failedCount}
        pageCount={payments.length}
        totalCount={paymentsQuery.data?.totalItems ?? 0}
      />

      <PaymentFilters filters={filters} onChange={setFilters} />

      <section className="table-section" aria-label="Lista de pagamentos">
        <PaymentTable
          isLoading={paymentsQuery.isLoading}
          onSelectError={handleSelectError}
          payments={payments}
          viewedErrorIds={viewedErrorIds}
        />
      </section>

      <PaymentErrorDetailPanel onClose={() => setSelectedErrorId(null)} paymentId={selectedErrorId} />
    </main>
  );
}
