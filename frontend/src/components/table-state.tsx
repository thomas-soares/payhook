import { Loader2 } from "lucide-react";

type TableStateProps = {
  isLoading: boolean;
  loadingLabel: string;
  emptyLabel: string;
};

export function TableState({ emptyLabel, isLoading, loadingLabel }: TableStateProps) {
  if (isLoading) {
    return (
      <div className="state-panel" role="status">
        <Loader2 aria-hidden="true" className="spin" size={22} />
        <span>{loadingLabel}</span>
      </div>
    );
  }

  return (
    <div className="state-panel">
      <span>{emptyLabel}</span>
    </div>
  );
}
