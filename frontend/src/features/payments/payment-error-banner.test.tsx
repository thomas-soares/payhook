import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { PaymentErrorBanner } from "./payment-error-banner";

describe("PaymentErrorBanner", () => {
  it("renders the error message as an alert", () => {
    render(<PaymentErrorBanner message="Nao foi possivel carregar os pagamentos." />);

    expect(screen.getByRole("alert")).toHaveTextContent("Nao foi possivel carregar os pagamentos.");
  });
});
