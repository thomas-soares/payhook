export type ProcessingStatus = "Pending" | "Processed" | "Failed";

export type PaymentStatusFilter = "all" | "Processed" | "Failed";

export type PaymentSummary = {
  id: string;
  transactionId: string;
  contractId: string;
  processingStatus: ProcessingStatus;
  receivedAt: string;
  paymentStatus: string | null;
  amount: number | null;
  paymentDate: string | null;
  processingError: string | null;
};

export type PaginatedPayments = {
  items: PaymentSummary[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
};

export type PaymentFilters = {
  status: PaymentStatusFilter;
  contractId: string;
};
