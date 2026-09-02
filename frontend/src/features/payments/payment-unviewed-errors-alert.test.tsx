import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { PaymentUnviewedErrorsAlert } from "./payment-unviewed-errors-alert";

describe("PaymentUnviewedErrorsAlert", () => {
  it("renders nothing when there are no unviewed errors", () => {
    const { container } = render(<PaymentUnviewedErrorsAlert count={0} />);

    expect(container).toBeEmptyDOMElement();
  });

  it("renders a singular alert", () => {
    render(<PaymentUnviewedErrorsAlert count={1} />);

    expect(screen.getByRole("alert")).toHaveTextContent("1 erro ainda nao visualizado");
  });

  it("renders a plural alert", () => {
    render(<PaymentUnviewedErrorsAlert count={2} />);

    expect(screen.getByRole("alert")).toHaveTextContent("2 erros ainda nao visualizados");
  });
});
