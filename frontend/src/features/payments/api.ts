import type { PaginatedPayments, PaymentFilters } from "./types";

export async function fetchPayments(filters: PaymentFilters): Promise<PaginatedPayments> {
  const searchParams = new URLSearchParams({
    page: "1",
    pageSize: "20"
  });

  if (filters.status !== "all") {
    searchParams.set("status", filters.status);
  }

  const contractId = filters.contractId.trim();

  if (contractId) {
    searchParams.set("contract_id", contractId);
  }

  const response = await fetch(`/api/payments?${searchParams.toString()}`, {
    headers: {
      Accept: "application/json"
    }
  });

  if (!response.ok) {
    throw new Error("Nao foi possivel carregar os pagamentos.");
  }

  return (await response.json()) as PaginatedPayments;
}
