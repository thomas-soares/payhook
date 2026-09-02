import { fireEvent, screen, waitFor } from "@testing-library/react";
import { http, HttpResponse } from "msw";
import { describe, expect, it, vi } from "vitest";
import { renderWithQueryClient } from "@/test/render";
import { server } from "@/test/server";
import { PaymentErrorDetailPanel } from "./payment-error-detail-panel";
import { createPaymentDetail } from "./test-data";

describe("PaymentErrorDetailPanel", () => {
  it("renders nothing without a selected payment", () => {
    const { container } = renderWithQueryClient(
      <PaymentErrorDetailPanel onClose={vi.fn()} paymentId={null} />
    );

    expect(container).toBeEmptyDOMElement();
  });

  it("loads and renders payment error detail", async () => {
    server.use(
      http.get("*/api/payments/payment-id", () =>
        HttpResponse.json(
          createPaymentDetail({
            id: "payment-id",
            processingError: "Invalid status"
          })
        )
      )
    );

    renderWithQueryClient(<PaymentErrorDetailPanel onClose={vi.fn()} paymentId="payment-id" />);

    expect(await screen.findByText("Invalid status")).toBeInTheDocument();
    expect(screen.getByText("tx-001")).toBeInTheDocument();
    expect(screen.getByText(/id_transacao/)).toBeInTheDocument();
  });

  it("renders raw payload when JSON parsing fails", async () => {
    server.use(
      http.get("*/api/payments/payment-id", () =>
        HttpResponse.json(
          createPaymentDetail({
            id: "payment-id",
            payloadJson: "{invalid"
          })
        )
      )
    );

    renderWithQueryClient(<PaymentErrorDetailPanel onClose={vi.fn()} paymentId="payment-id" />);

    expect(await screen.findByText("{invalid")).toBeInTheDocument();
  });

  it("renders a detail loading error", async () => {
    server.use(http.get("*/api/payments/payment-id", () => HttpResponse.json({}, { status: 500 })));

    renderWithQueryClient(<PaymentErrorDetailPanel onClose={vi.fn()} paymentId="payment-id" />);

    expect(await screen.findByRole("alert")).toHaveTextContent(
      "Nao foi possivel carregar o detalhe do pagamento."
    );
  });

  it("calls close from the close button", async () => {
    const onClose = vi.fn();

    server.use(
      http.get("*/api/payments/payment-id", () =>
        HttpResponse.json(createPaymentDetail({ id: "payment-id" }))
      )
    );

    renderWithQueryClient(<PaymentErrorDetailPanel onClose={onClose} paymentId="payment-id" />);

    fireEvent.click(screen.getByRole("button", { name: "Fechar detalhe" }));

    await waitFor(() => expect(onClose).toHaveBeenCalled());
  });
});
