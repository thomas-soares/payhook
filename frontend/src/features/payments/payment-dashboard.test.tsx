import { fireEvent, render, screen, waitFor, within } from "@testing-library/react";
import { http, HttpResponse } from "msw";
import { describe, expect, it } from "vitest";
import { renderWithQueryClient } from "@/test/render";
import { server } from "@/test/server";
import { PaymentDashboard } from "./payment-dashboard";
import { createPaymentSummary } from "./test-data";

function renderDashboard() {
  return renderWithQueryClient(<PaymentDashboard pollIntervalMs={false} />);
}

describe("PaymentDashboard", () => {
  it("lists payments returned by the API", async () => {
    server.use(
      http.get("*/api/payments", () =>
        HttpResponse.json({
          items: [
            createPaymentSummary()
          ],
          page: 1,
          pageSize: 20,
          totalItems: 1,
          totalPages: 1
        })
      )
    );

    renderDashboard();

    const row = await screen.findByRole("row", { name: /sucesso tx-001 contract-001/i });

    expect(within(row).getByText("tx-001")).toBeInTheDocument();
    expect(within(row).getByText("contract-001")).toBeInTheDocument();
    expect(within(row).getAllByText("Sucesso")).toHaveLength(2);
  });

  it("sends status and contract filters as query parameters", async () => {
    const requests: string[] = [];

    server.use(
      http.get("*/api/payments", ({ request }) => {
        requests.push(new URL(request.url).search);

        return HttpResponse.json({
          items: [],
          page: 1,
          pageSize: 20,
          totalItems: 0,
          totalPages: 0
        });
      })
    );

    renderDashboard();

    await screen.findByText("Nenhum pagamento encontrado.");

    fireEvent.click(screen.getByRole("button", { name: "Erro" }));
    fireEvent.change(screen.getByPlaceholderText("ID do contrato"), {
      target: { value: "contract-009" }
    });

    await waitFor(() => {
      expect(requests.some((search) => search.includes("status=Failed"))).toBe(true);
      expect(requests.some((search) => search.includes("contract_id=contract-009"))).toBe(true);
    });
  });

  it("shows the empty state when no payments match", async () => {
    server.use(
      http.get("*/api/payments", () =>
        HttpResponse.json({
          items: [],
          page: 1,
          pageSize: 20,
          totalItems: 0,
          totalPages: 0
        })
      )
    );

    renderDashboard();

    expect(await screen.findByText("Nenhum pagamento encontrado.")).toBeInTheDocument();
  });

  it("shows an alert when loading payments fails", async () => {
    server.use(http.get("*/api/payments", () => HttpResponse.json({}, { status: 502 })));

    renderDashboard();

    expect(await screen.findByRole("alert")).toHaveTextContent(
      "Nao foi possivel carregar os pagamentos."
    );
  });

  it("shows a visual error indicator for failed payments", async () => {
    server.use(
      http.get("*/api/payments", () =>
        HttpResponse.json({
          items: [
            createPaymentSummary({
              amount: null,
              contractId: "contract-err",
              id: "dd974fab-914d-4081-83a4-108a207b2fe1",
              paymentDate: null,
              paymentStatus: null,
              processingError: "Invalid status",
              transactionId: "tx-error",
              processingStatus: "Failed"
            })
          ],
          page: 1,
          pageSize: 20,
          totalItems: 1,
          totalPages: 1
        })
      )
    );

    renderDashboard();

    const row = await screen.findByRole("row", { name: /erro tx-error contract-err/i });

    expect(within(row).getByText("Erro")).toBeInTheDocument();
    expect(screen.getByText("Com erro")).toBeInTheDocument();
  });
});
