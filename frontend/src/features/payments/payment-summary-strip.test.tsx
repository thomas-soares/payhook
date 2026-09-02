import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { PaymentSummaryStrip } from "./payment-summary-strip";

describe("PaymentSummaryStrip", () => {
  it("renders summary totals", () => {
    render(<PaymentSummaryStrip failedCount={2} pageCount={10} totalCount={42} />);

    expect(screen.getByText("Total")).toBeInTheDocument();
    expect(screen.getByText("42")).toBeInTheDocument();
    expect(screen.getByText("Nesta pagina")).toBeInTheDocument();
    expect(screen.getByText("10")).toBeInTheDocument();
    expect(screen.getByText("Com erro")).toBeInTheDocument();
    expect(screen.getByText("2")).toBeInTheDocument();
  });
});
