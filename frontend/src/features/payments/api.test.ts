import { afterEach, describe, expect, it, vi } from "vitest";
import { fetchPaymentDetail, fetchPayments } from "./api";

describe("fetchPayments", () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("builds query parameters from filters", async () => {
    const fetchMock = vi.spyOn(globalThis, "fetch").mockResolvedValue(
      new Response(
        JSON.stringify({
          items: [],
          page: 1,
          pageSize: 20,
          totalItems: 0,
          totalPages: 0
        }),
        { status: 200 }
      )
    );

    await fetchPayments({ contractId: " contract-001 ", status: "Failed" });

    expect(fetchMock).toHaveBeenCalledWith(
      "/api/payments?page=1&pageSize=20&status=Failed&contract_id=contract-001",
      {
        headers: {
          Accept: "application/json"
        }
      }
    );
  });

  it("throws when the request fails", async () => {
    vi.spyOn(globalThis, "fetch").mockResolvedValue(new Response(null, { status: 502 }));

    await expect(fetchPayments({ contractId: "", status: "all" })).rejects.toThrow(
      "Nao foi possivel carregar os pagamentos."
    );
  });
});

describe("fetchPaymentDetail", () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("fetches a payment detail by id", async () => {
    const fetchMock = vi.spyOn(globalThis, "fetch").mockResolvedValue(
      new Response(
        JSON.stringify({
          id: "payment-id",
          transactionId: "tx-001",
          contractId: "contract-001",
          processingStatus: "Failed",
          receivedAt: "2026-09-01T18:00:00Z",
          payloadJson: "{}",
          paymentStatus: null,
          amount: null,
          paymentDate: null,
          updatedAt: null,
          processingError: "Invalid status"
        }),
        { status: 200 }
      )
    );

    await fetchPaymentDetail("payment-id");

    expect(fetchMock).toHaveBeenCalledWith("/api/payments/payment-id", {
      headers: {
        Accept: "application/json"
      }
    });
  });

  it("throws when the detail request fails", async () => {
    vi.spyOn(globalThis, "fetch").mockResolvedValue(new Response(null, { status: 404 }));

    await expect(fetchPaymentDetail("missing-id")).rejects.toThrow(
      "Nao foi possivel carregar o detalhe do pagamento."
    );
  });
});
