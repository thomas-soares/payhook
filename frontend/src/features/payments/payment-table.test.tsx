import { fireEvent, render, screen, within } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { PaymentTable } from "./payment-table";
import { createPaymentSummary } from "./test-data";

function renderTable(payments = [createPaymentSummary()]) {
  const onSelectError = vi.fn();

  render(
    <PaymentTable
      isLoading={false}
      onSelectError={onSelectError}
      payments={payments}
      viewedErrorIds={new Set()}
    />
  );

  return { onSelectError };
}

describe("PaymentTable", () => {
  it("renders loading state", () => {
    render(
      <PaymentTable
        isLoading={true}
        onSelectError={vi.fn()}
        payments={[]}
        viewedErrorIds={new Set()}
      />
    );

    expect(screen.getByRole("status")).toHaveTextContent("Carregando pagamentos...");
  });

  it("renders empty state", () => {
    render(
      <PaymentTable
        isLoading={false}
        onSelectError={vi.fn()}
        payments={[]}
        viewedErrorIds={new Set()}
      />
    );

    expect(screen.getByText("Nenhum pagamento encontrado.")).toBeInTheDocument();
  });

  it("renders payment rows", () => {
    renderTable();

    const row = screen.getByRole("row", { name: /sucesso tx-001 contract-001/i });

    expect(within(row).getByText("tx-001")).toBeInTheDocument();
    expect(within(row).getByText("contract-001")).toBeInTheDocument();
    expect(within(row).getAllByText("Sucesso")).toHaveLength(2);
  });

  it("uses placeholders for missing payment data", () => {
    renderTable([
      createPaymentSummary({
        amount: null,
        paymentStatus: null
      })
    ]);

    const row = screen.getByRole("row", { name: /sucesso tx-001 contract-001/i });

    expect(within(row).getAllByText("--")).toHaveLength(3);
  });

  it("emits failed payment selection", () => {
    const { onSelectError } = renderTable([
      createPaymentSummary({
        id: "failed-payment-id",
        processingStatus: "Failed"
      })
    ]);

    fireEvent.click(screen.getByRole("button", { name: "Ver erro" }));

    expect(onSelectError).toHaveBeenCalledWith("failed-payment-id");
  });

  it("marks viewed errors", () => {
    render(
      <PaymentTable
        isLoading={false}
        onSelectError={vi.fn()}
        payments={[
          createPaymentSummary({
            id: "failed-payment-id",
            processingStatus: "Failed"
          })
        ]}
        viewedErrorIds={new Set(["failed-payment-id"])}
      />
    );

    expect(screen.getByRole("button", { name: "Visto" })).toBeInTheDocument();
  });
});
