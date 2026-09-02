import { render, screen, within } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { PaymentTable } from "./payment-table";
import { createPaymentSummary } from "./test-data";

describe("PaymentTable", () => {
  it("renders loading state", () => {
    render(<PaymentTable isLoading={true} payments={[]} />);

    expect(screen.getByRole("status")).toHaveTextContent("Carregando pagamentos...");
  });

  it("renders empty state", () => {
    render(<PaymentTable isLoading={false} payments={[]} />);

    expect(screen.getByText("Nenhum pagamento encontrado.")).toBeInTheDocument();
  });

  it("renders payment rows", () => {
    render(<PaymentTable isLoading={false} payments={[createPaymentSummary()]} />);

    const row = screen.getByRole("row", { name: /sucesso tx-001 contract-001/i });

    expect(within(row).getByText("tx-001")).toBeInTheDocument();
    expect(within(row).getByText("contract-001")).toBeInTheDocument();
    expect(within(row).getAllByText("Sucesso")).toHaveLength(2);
  });

  it("uses placeholders for missing payment data", () => {
    render(
      <PaymentTable
        isLoading={false}
        payments={[
          createPaymentSummary({
            amount: null,
            paymentStatus: null
          })
        ]}
      />
    );

    const row = screen.getByRole("row", { name: /sucesso tx-001 contract-001/i });

    expect(within(row).getAllByText("--")).toHaveLength(2);
  });
});
