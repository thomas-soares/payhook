import { describe, expect, it } from "vitest";
import { cn } from "./utils";

describe("cn", () => {
  it("merges conditional and conflicting class names", () => {
    expect(cn("px-2", "px-4", { hidden: false, block: true })).toBe("px-4 block");
  });
});
