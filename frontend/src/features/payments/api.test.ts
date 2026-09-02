import { afterEach, describe, expect, it, vi } from "vitest";
import { fetchPayments } from "./api";

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
