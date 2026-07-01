// Tipos que reflejan los DTOs del backend .NET

export type UserType = "Administrator" | "Client";

export interface UserDto {
  id: string;
  userName: string;
  email: string;
  name: string;
  userType: UserType;
  clientId?: string | null;
  isActive: boolean;
  emailConfirmed: boolean;
  lastLoginAt?: string | null;
  roles: string[];
  permissions: string[];
}

export interface AuthResponseDto {
  accessToken: string;
  user: UserDto;
}

export interface RoleDto {
  id: string;
  name: string;
  description?: string | null;
  userType?: string | null;
}

export interface RoleDetailDto extends RoleDto {
  permissions: PermissionDto[];
}

export interface PermissionDto {
  id: string;
  code: string;
  name: string;
  description?: string | null;
}

export interface PaginatedList<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface CreateUserRequest {
  userName: string;
  email: string;
  password: string;
  name: string;
  userType: UserType;
  clientId?: string | null;
}

export interface UpdateUserRequest {
  name: string;
  email: string;
  userType: UserType;
  clientId?: string | null;
}

export interface AssignRolesRequest {
  roleIds: string[];
}

export interface CreateRoleRequest {
  name: string;
  description?: string | null;
  userType?: string | null;
}

export interface UpdateRoleRequest {
  name: string;
  description?: string | null;
  userType?: string | null;
}

export interface AssignPermissionsRequest {
  permissionIds: string[];
}

export interface CreatePermissionRequest {
  code: string;
  name: string;
  description?: string | null;
}

export interface UpdatePermissionRequest {
  code: string;
  name: string;
  description?: string | null;
}

export interface ApiError {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  errors?: Record<string, string[]>;
  message?: string;
}

export interface GetUsersParams {
  searchTerm?: string;
  userType?: UserType;
  isActive?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

// ── CLIENTS ─────────────────────────────────────────────────────────────────

export type ClientStatus = "Active" | "Inactive" | "Prospective";

export interface ClientTagDto {
  id: string;
  name: string;
  color: string;
}

export interface ClientCategoryDto {
  id: string;
  name: string;
  description?: string | null;
}

export interface ClientContactDto {
  id: string;
  clientId: string;
  name: string;
  email?: string | null;
  phone?: string | null;
  position?: string | null;
  isPrimary: boolean;
}

export interface ClientNoteDto {
  id: string;
  clientId: string;
  content: string;
  isPinned: boolean;
  createdAt: string;
}

export interface ClientListDto {
  id: string;
  code: string;
  name: string;
  tradeName?: string | null;
  taxId?: string | null;
  status: ClientStatus;
  categoryName?: string | null;
  primaryContactName?: string | null;
  tags: ClientTagDto[];
}

export interface ClientDetailDto {
  id: string;
  code: string;
  name: string;
  tradeName?: string | null;
  taxId?: string | null;
  billingEmail?: string | null;
  billingAddress?: string | null;
  phone?: string | null;
  status: ClientStatus;
  notes?: string | null;
  categoryId?: string | null;
  categoryName?: string | null;
  tags: ClientTagDto[];
  contacts: ClientContactDto[];
  notesList: ClientNoteDto[];
}

export interface ClientDto extends Omit<ClientDetailDto, "contacts" | "notesList"> {}

export interface GetClientsParams {
  searchTerm?: string;
  status?: ClientStatus;
  categoryId?: string;
  tagId?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface CreateClientRequest {
  name: string;
  tradeName?: string | null;
  taxId?: string | null;
  billingEmail?: string | null;
  billingAddress?: string | null;
  phone?: string | null;
  status: ClientStatus;
  notes?: string | null;
  categoryId?: string | null;
  tagIds?: string[];
}

export interface UpdateClientRequest extends CreateClientRequest {}

export interface AddContactRequest {
  name: string;
  email?: string | null;
  phone?: string | null;
  position?: string | null;
  isPrimary?: boolean;
}

export interface AddNoteRequest {
  content: string;
  isPinned?: boolean;
}

export interface CreateTagRequest {
  name: string;
  color: string;
}

export interface UpdateTagRequest extends CreateTagRequest {}

export interface CreateCategoryRequest {
  name: string;
  description?: string | null;
}

export interface UpdateCategoryRequest extends CreateCategoryRequest {}

// ── SERVICES ─────────────────────────────────────────────────────────────────

export type BillingType = "Monthly" | "Yearly" | "OneTime" | "Hourly" | "Custom";

export interface ServiceCategoryDto {
  id: string;
  name: string;
  description?: string | null;
}

export interface ServicePriceHistoryDto {
  id: string;
  serviceId: string;
  price: number;
  currency: string;
  effectiveDate: string;
  reason?: string | null;
  createdAt: string;
}

export interface ServiceListDto {
  id: string;
  code: string;
  name: string;
  categoryName?: string | null;
  billingType: string;
  defaultPrice: number;
  currency: string;
  isActive: boolean;
  isPublic: boolean;
}

export interface ServiceDetailDto {
  id: string;
  code: string;
  name: string;
  description?: string | null;
  categoryId?: string | null;
  categoryName?: string | null;
  billingType: string;
  defaultPrice: number;
  currency: string;
  isActive: boolean;
  isPublic: boolean;
  priceHistory: ServicePriceHistoryDto[];
}

export interface ServiceDto extends Omit<ServiceDetailDto, "priceHistory"> {}

export interface GetServicesParams {
  searchTerm?: string;
  categoryId?: string;
  billingType?: BillingType;
  isActive?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

export interface CreateServiceRequest {
  name: string;
  description?: string | null;
  categoryId?: string | null;
  billingType: BillingType;
  defaultPrice: number;
  currency: string;
  isActive: boolean;
  isPublic: boolean;
}

export interface UpdateServiceRequest extends CreateServiceRequest {}

export interface AddPriceHistoryRequest {
  price: number;
  currency: string;
  effectiveDate: string;
  reason?: string | null;
}

export interface CreateServiceCategoryRequest {
  name: string;
  description?: string | null;
}

export interface UpdateServiceCategoryRequest extends CreateServiceCategoryRequest {}

// ── SUBSCRIPTIONS ─────────────────────────────────────────────────────────────

export type SubscriptionStatus =
  | "Pending"
  | "Active"
  | "Suspended"
  | "Cancelled"
  | "Expired";

export interface SubscriptionPriceHistoryDto {
  id: string;
  subscriptionId: string;
  oldPrice: number;
  newPrice: number;
  effectiveDate: string;
  reason?: string | null;
  createdAt: string;
}

export interface SubscriptionChangeLogDto {
  id: string;
  subscriptionId: string;
  changeType: string;
  oldValue?: string | null;
  newValue?: string | null;
  reason?: string | null;
  createdAt: string;
}

export interface SubscriptionPermissionDto {
  id: string;
  userId: string;
  userName: string;
  subscriptionId: string;
  subscriptionCode: string;
  serviceName: string;
}

export interface SubscriptionListDto {
  id: string;
  code: string;
  clientName: string;
  serviceName: string;
  billingType: string;
  price: number;
  currency: string;
  startDate: string;
  endDate?: string | null;
  status: SubscriptionStatus;
  autoRenew: boolean;
  nextBillingDate?: string | null;
}

export interface SubscriptionDetailDto {
  id: string;
  code: string;
  clientId: string;
  clientName: string;
  serviceId: string;
  serviceName: string;
  billingType: string;
  price: number;
  currency: string;
  startDate: string;
  endDate?: string | null;
  billingDay: number;
  status: SubscriptionStatus;
  autoRenew: boolean;
  notes?: string | null;
  lastBillingDate?: string | null;
  nextBillingDate?: string | null;
  priceHistory: SubscriptionPriceHistoryDto[];
  changeLogs: SubscriptionChangeLogDto[];
}

export interface SubscriptionDto
  extends Omit<SubscriptionDetailDto, "priceHistory" | "changeLogs"> {}

export interface GetSubscriptionsParams {
  clientId?: string;
  serviceId?: string;
  status?: SubscriptionStatus;
  startDateFrom?: string;
  startDateTo?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface CreateSubscriptionRequest {
  clientId: string;
  serviceId: string;
  billingType: BillingType;
  price: number;
  currency: string;
  startDate: string;
  endDate?: string | null;
  billingDay: number;
  autoRenew: boolean;
  notes?: string | null;
}

export interface UpdateSubscriptionRequest extends CreateSubscriptionRequest {}

export interface ChangePriceRequest {
  newPrice: number;
  effectiveDate: string;
  reason?: string | null;
}

export interface SubscriptionActionRequest {
  reason?: string | null;
  effectiveDate?: string | null;
}

export interface AddSubscriptionPermissionRequest {
  userId: string;
}

// ── BILLING ───────────────────────────────────────────────────────────────────

export type BillingCycleStatus = "Open" | "Closed" | "Reprocessing";
export type BillingItemStatus = "Pending" | "Partial" | "Paid" | "Cancelled";

export interface BillingCycleDto {
  id: string;
  year: number;
  month?: number | null;
  startDate: string;
  endDate: string;
  status: BillingCycleStatus;
  closedAt?: string | null;
  closedBy?: string | null;
  itemsCount: number;
}

export interface BillingItemListDto {
  id: string;
  subscriptionCode: string;
  clientName: string;
  description: string;
  amount: number;
  currency: string;
  dueDate: string;
  status: BillingItemStatus;
  paidAmount: number;
  balance: number;
  cancellationReason?: string | null;
}

export interface BillingGenerationLogDto {
  id: string;
  billingCycleId?: string | null;
  billingCycleLabel?: string | null;
  startedAt: string;
  finishedAt?: string | null;
  status: string;
  itemsGenerated: number;
  errors?: string | null;
  triggeredBy?: string | null;
}

export interface BillingGenerationResult {
  success: boolean;
  itemsGenerated: number;
  errors: string[];
}

export interface GetBillingCyclesParams {
  year?: number;
  month?: number;
}

export interface GetBillingItemsParams {
  billingCycleId?: string;
  clientId?: string;
  status?: BillingItemStatus;
  dueDateFrom?: string;
  dueDateTo?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface GetBillingLogsParams {
  billingCycleId?: string;
  year?: number;
  month?: number;
  limit?: number;
}

export interface GenerateBillingRequest {
  year: number;
  month?: number | null;
}

export interface GenerateForSubscriptionRequest {
  subscriptionId: string;
}

export interface CancelBillingItemRequest {
  reason: string;
}

// ── LEDGER ────────────────────────────────────────────────────────────────────

export type LedgerEntryType =
  | "Income"
  | "Expense"
  | "TransferIn"
  | "TransferOut"
  | "Adjustment";

export interface LedgerEntryListDto {
  id: string;
  bankAccountId: string;
  bankAccountName: string;
  entryType: LedgerEntryType;
  amount: number;
  currency: string;
  date: string;
  description: string;
  reference?: string | null;
  clientId?: string | null;
  clientName?: string | null;
  isReconciled: boolean;
}

export interface LedgerEntryDto extends LedgerEntryListDto {
  billingItemId?: string | null;
  billingItemDescription?: string | null;
  reconciledAt?: string | null;
}

export interface TransferGroupDto {
  id: string;
  fromEntryId: string;
  fromBankAccountId: string;
  fromBankAccountName: string;
  toEntryId: string;
  toBankAccountId: string;
  toBankAccountName: string;
  amount: number;
  date: string;
  description?: string | null;
}

export interface GetLedgerEntriesParams {
  bankAccountId?: string;
  entryType?: LedgerEntryType;
  dateFrom?: string;
  dateTo?: string;
  clientId?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface RegisterIncomeRequest {
  bankAccountId: string;
  amount: number;
  currency: string;
  date: string;
  description: string;
  reference?: string | null;
  clientId?: string | null;
  billingItemId?: string | null;
}

export interface RegisterExpenseRequest {
  bankAccountId: string;
  amount: number;
  currency: string;
  date: string;
  description: string;
  reference?: string | null;
}

export interface RegisterTransferRequest {
  fromBankAccountId: string;
  toBankAccountId: string;
  amount: number;
  currency: string;
  date: string;
  description?: string | null;
}

export interface RegisterAdjustmentRequest {
  bankAccountId: string;
  amount: number;
  currency: string;
  date: string;
  description: string;
  reason: string;
}

// ── BANK ACCOUNTS ─────────────────────────────────────────────────────────────

export interface BankAccountListDto {
  id: string;
  name: string;
  bankName?: string | null;
  maskedAccountNumber?: string | null;
  currency: string;
  openingBalance: number;
  isActive: boolean;
}

export interface BankAccountDto extends BankAccountListDto {
  openingDate: string;
}

export interface GetBankAccountsParams {
  isActive?: boolean;
  search?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface CreateBankAccountRequest {
  name: string;
  bankName?: string | null;
  accountNumber?: string | null;
  currency: string;
  openingBalance: number;
  openingDate: string;
  isActive: boolean;
}

export interface UpdateBankAccountRequest extends CreateBankAccountRequest {}

// ── ACCOUNT STATEMENTS ────────────────────────────────────────────────────────

export interface AccountStatementItemDto {
  date: string;
  type: string;
  description: string;
  referenceId: string;
  debit: number;
  credit: number;
  balance: number;
  originalAmountMXN: number;
  exchangeRateUsed?: number | null;
}

export interface AccountStatementDto {
  clientId: string;
  clientName: string;
  statementDate: string;
  startDate?: string | null;
  endDate?: string | null;
  displayCurrency: string;
  exchangeRateUsed?: number | null;
  initialBalance: number;
  totalCharges: number;
  totalPayments: number;
  totalAdjustments: number;
  finalBalance: number;
  items: AccountStatementItemDto[];
}

export interface FinancialSummaryDto {
  clientId: string;
  clientName: string;
  displayCurrency: string;
  exchangeRateUsed?: number | null;
  totalCharges: number;
  totalPayments: number;
  totalAdjustments: number;
  currentBalance: number;
}

export interface GetClientStatementParams {
  from?: string;
  to?: string;
  currency?: string;
}

// ── ALLOCATIONS ───────────────────────────────────────────────────────────────

export interface SubscriptionAllocationDto {
  id: string;
  ledgerEntryId: string;
  billingItemId: string;
  amount: number;
  allocatedAt: string;
  allocatedBy?: string | null;
  isAutomatic: boolean;
  billingItemDescription?: string | null;
  ledgerEntryDescription?: string | null;
}

export interface AllocationResult {
  success: boolean;
  allocatedAmount: number;
  remainingAmount: number;
  errors: string[];
}

export interface ManualAllocationItem {
  billingItemId: string;
  amount: number;
}

export interface ManualAllocateRequest {
  ledgerEntryId: string;
  allocations: ManualAllocationItem[];
}
