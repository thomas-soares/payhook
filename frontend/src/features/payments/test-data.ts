import type { PaymentSummary } from "./types";

export function createPaymentSummary(overrides: Partial<PaymentSummary> = {}): PaymentSummary {
  return {
    amount: 199.9,
    contractId: "contract-001",
    id: "b4c9da61-6fa9-4c47-9a12-62f73c87a199",
    paymentDate: "2026-09-01T18:00:00Z",
    paymentStatus: "Sucesso",
    processingError: null,
    processingStatus: "Processed",
    receivedAt: "2026-09-01T18:00:00Z",
    transactionId: "tx-001",
    ...overrides
  };
}
