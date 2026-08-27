// Tipos que reflejan los DTOs del backend .NET

export type UserType = "Administrator" | "Client";

export interface UserDto {
  id: string;
  userName: string;
  email: string;
  name: string;
  userType: UserType;
  clientId?: string | null;
  organizationId?: string | null;
  isActive: boolean;
  emailConfirmed: boolean;
  lastLoginAt?: string | null;
  lockoutEnd?: string | null;
  mustChangePassword: boolean;
  activeSessionCount: number;
  roles: string[];
  permissions: string[];
  uiPolicies: Record<string, UiAccessMode>;
}

export type UiAccessMode = "Hidden" | "ReadOnly" | "Disabled" | "Enabled";

export interface UiControlDefinitionDto {
  controlKey: string;
  label: string;
  module: string;
  controlType: string;
  requiredPermissionCode?: string | null;
  isSystemRequired: boolean;
}

export interface UiControlCatalogItem extends UiControlDefinitionDto {
  id: string;
}

export interface UiControlPolicyRequest {
  uiControlDefinitionId: string;
  roleId?: string | null;
  userId?: string | null;
  clientId?: string | null;
  subscriptionId?: string | null;
  accessMode: UiAccessMode;
  expiresAt?: string | null;
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
  module: string;
  action: string;
  kind: string;
  isSystemGenerated: boolean;
  isActive: boolean;
}

export interface AuditLogDto {
  id: string;
  entityType: string;
  entityId: string;
  action: string;
  changes: string;
  userId?: string | null;
  userName?: string | null;
  timestamp: string;
  ipAddress?: string | null;
  userAgent?: string | null;
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

export interface UserSessionDto {
  id: string;
  deviceName: string;
  ipAddress?: string | null;
  lastSeenAt: string;
  expiresAt: string;
  revokedAt?: string | null;
  isActive: boolean;
}

export interface UserPermissionOverrideDto {
  id: string;
  permissionId: string;
  permissionCode: string;
  permissionName: string;
  isGranted: boolean;
  clientId?: string | null;
  subscriptionId?: string | null;
  expiresAt?: string | null;
}

export interface SetUserPermissionRequest {
  permissionId: string;
  isGranted: boolean;
  clientId?: string | null;
  subscriptionId?: string | null;
  expiresAt?: string | null;
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
  module: string;
  action: string;
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
  presentationCurrency: string;
  creditLimit: number;
  accountManagerName?: string | null;
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
  documents: ClientDocumentDto[];
  presentationCurrency: string;
  paymentTermsDays: number;
  creditLimit: number;
  commercialTerms?: string | null;
  accountManagerUserId?: string | null;
  accountManagerName?: string | null;
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
  presentationCurrency: string;
  paymentTermsDays: number;
  creditLimit: number;
  commercialTerms?: string | null;
  accountManagerUserId?: string | null;
}

export interface UpdateClientRequest extends CreateClientRequest {}

export interface ClientDocumentDto { id: string; clientId: string; name: string; contentType: string; sizeBytes: number; description?: string | null; createdAt: string; }
export interface ClientTimelineItemDto { id: string; occurredAt: string; type: string; title: string; detail?: string | null; actorUserId?: string | null; }
export interface ClientDuplicateRuleDto { matchTaxId: boolean; matchName: boolean; matchBillingEmail: boolean; blockOnMatch: boolean; }
export interface ClientDuplicateMatchDto { id: string; code: string; name: string; matchedFields: string[]; }

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

export type ProrationPolicy = "None" | "Daily";

export interface ServiceVersionConceptDto {
  id: string;
  code: string;
  name: string;
  description?: string | null;
  quantity: number;
  unitPrice: number;
  taxPercent: number;
  sortOrder: number;
}

export interface ServiceVersionDto {
  id: string;
  serviceId: string;
  versionNumber: number;
  name: string;
  description?: string | null;
  billingType: BillingType;
  basePrice: number;
  currency: string;
  defaultDiscountPercent: number;
  defaultTaxPercent: number;
  customIntervalDays?: number | null;
  prorationPolicy: ProrationPolicy;
  effectiveFrom: string;
  effectiveTo?: string | null;
  terms?: string | null;
  isPublished: boolean;
  concepts: ServiceVersionConceptDto[];
}

export interface CreateServiceVersionRequest {
  name: string;
  description?: string | null;
  billingType: BillingType;
  basePrice: number;
  currency: string;
  defaultDiscountPercent: number;
  defaultTaxPercent: number;
  customIntervalDays?: number | null;
  prorationPolicy: ProrationPolicy;
  effectiveFrom: string;
  terms?: string | null;
  isPublished: boolean;
  concepts: Array<Omit<ServiceVersionConceptDto, "id">>;
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
  serviceVersionId?: string | null;
  customIntervalDays?: number | null;
  discountPercent: number;
  taxPercent: number;
  prorationPolicy: ProrationPolicy;
  contractTerms?: string | null;
  status: SubscriptionStatus;
  autoRenew: boolean;
  notes?: string | null;
  lastBillingDate?: string | null;
  nextBillingDate?: string | null;
  priceHistory: SubscriptionPriceHistoryDto[];
  changeLogs: SubscriptionChangeLogDto[];
  termsVersions: SubscriptionTermsVersionDto[];
}

export interface SubscriptionDto
  extends Omit<SubscriptionDetailDto, "priceHistory" | "changeLogs" | "termsVersions"> {}

export interface SubscriptionTermsVersionDto {
  id: string;
  versionNumber: number;
  serviceVersionId?: string | null;
  effectiveFrom: string;
  effectiveTo?: string | null;
  billingType: BillingType;
  basePrice: number;
  currency: string;
  discountPercent: number;
  taxPercent: number;
  billingDay: number;
  customIntervalDays?: number | null;
  prorationPolicy: ProrationPolicy;
  terms?: string | null;
  reason?: string | null;
  total: number;
}

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
  customIntervalDays?: number | null;
  discountPercent: number;
  taxPercent: number;
  prorationPolicy: ProrationPolicy;
  contractTerms?: string | null;
  autoRenew: boolean;
  notes?: string | null;
}

export interface UpdateSubscriptionRequest extends CreateSubscriptionRequest {}

export interface ChangePriceRequest {
  newPrice: number;
  effectiveDate: string;
  reason?: string | null;
}

export interface ChangeSubscriptionPlanRequest {
  serviceVersionId: string;
  effectiveDate: string;
  reason?: string | null;
}

export interface SubscriptionActionRequest {
  reason?: string | null;
  effectiveDate?: string | null;
  idempotencyKey?: string;
}

export interface AddSubscriptionPermissionRequest {
  userId: string;
}

// ── BILLING ───────────────────────────────────────────────────────────────────

export type BillingCycleStatus = "Open" | "Closed" | "Reprocessing";
export type BillingItemStatus = "Pending" | "Partial" | "Paid" | "Settled" | "Cancelled";

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
  type: "SubscriptionCharge" | "ManualCharge";
  amount: number;
  baseAmount: number;
  discountAmount: number;
  taxAmount: number;
  prorationFactor: number;
  currency: string;
  dueDate: string;
  periodStart: string;
  periodEnd: string;
  status: BillingItemStatus;
  paidAmount: number;
  creditAmount: number;
  lateFeeAmount: number;
  balance: number;
  activePromise?: PaymentPromiseDto | null;
  cancellationReason?: string | null;
}

export interface PaymentPromiseDto { id: string; promisedAmount: number; promiseDate: string; status: "Pending" | "Fulfilled" | "Broken" | "Cancelled"; notes?: string | null; resolvedAt?: string | null; }
export interface BillingAdjustmentDto { id: string; type: "CreditNote" | "LateFee"; amount: number; reason: string; effectiveDate: string; reversedAt?: string | null; }
export interface AgingBucketDto { key: string; label: string; count: number; amount: number; }
export interface ReceivablesAgingDto { asOfDate: string; currency: string; totalOutstanding: number; ledgerBalance: number; difference: number; isReconciled: boolean; buckets: AgingBucketDto[]; }
export interface CreateManualChargeRequest { subscriptionId: string; amount: number; currency: string; description: string; chargeDate: string; dueDate: string; periodStart: string; periodEnd: string; idempotencyKey: string; }
export interface AddBillingAdjustmentRequest { type: "CreditNote" | "LateFee"; amount: number; effectiveDate: string; reason: string; idempotencyKey: string; }
export interface CreatePaymentPromiseRequest { promisedAmount: number; promiseDate: string; notes?: string | null; }

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
  currency?: string;
  outstandingOnly?: boolean;
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
  allocatedAmount: number;
  unappliedAmount: number;
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
  currency?: string;
  isReconciled?: boolean;
  hasUnappliedBalance?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

export interface GeneralLedgerAccountDto {
  id: string;
  code: string;
  name: string;
  accountType: "Asset" | "Liability" | "Equity" | "Revenue" | "Expense";
  purpose: string;
  currency: string;
  isSystem: boolean;
  isActive: boolean;
  parentAccountId?: string | null;
  bankAccountId?: string | null;
  balance: number;
}

export interface JournalLineDto {
  id: string;
  accountId: string;
  accountCode: string;
  accountName: string;
  debit: number;
  credit: number;
  description?: string | null;
  clientId?: string | null;
  bankAccountId?: string | null;
  billingItemId?: string | null;
  legacyLedgerEntryId?: string | null;
}

export interface JournalEntryDto {
  id: string;
  entryNumber: string;
  date: string;
  description: string;
  reference?: string | null;
  currency: string;
  status: "Draft" | "Posted" | "Reversed";
  sourceType: string;
  sourceId?: string | null;
  accountingPeriodId: string;
  accountingPeriodName: string;
  clientId?: string | null;
  postedAt?: string | null;
  reversedAt?: string | null;
  reversesJournalEntryId?: string | null;
  totalDebit: number;
  totalCredit: number;
  lines: JournalLineDto[];
}

export interface AccountingPeriodDto {
  id: string;
  name: string;
  startDate: string;
  endDate: string;
  status: "Open" | "Closed";
  closedAt?: string | null;
  closedBy?: string | null;
  closeVerificationCode?: string | null;
  reopenedAt?: string | null;
  reopenedBy?: string | null;
  reopenReason?: string | null;
  governanceVersion: number;
}

export interface AccountingCloseCheckDto { key: string; label: string; state: "Passed" | "Warning" | "Blocked"; count: number; detail: string; blocksClose: boolean; }
export interface AccountingPeriodChecklistDto { periodId: string; periodName: string; canClose: boolean; verificationCode: string; items: AccountingCloseCheckDto[]; }
export interface AccountingIntegrityAlertDto { key: string; severity: "Critical" | "Warning"; title: string; detail: string; count: number; }
export interface AccountingEvidenceSummaryDto { auditEvents: number; accessEvents: number; committedImports: number; failedImports: number; rolledBackImports: number; automaticReconciliations: number; journalReversals: number; paymentReversals: number; lastEventAt?: string | null; }
export interface AccountingPeriodReopenRequestDto { id: string; periodId: string; periodName: string; reason: string; status: "Pending" | "Approved" | "Rejected"; requestedAt: string; requestedBy: string; requestedByName?: string | null; reviewedAt?: string | null; reviewedBy?: string | null; reviewedByName?: string | null; reviewComment?: string | null; }

export interface TrialBalanceLineDto {
  accountId: string;
  accountCode: string;
  accountName: string;
  accountType: string;
  debit: number;
  credit: number;
  balance: number;
}

export interface TrialBalanceDto {
  asOfDate: string;
  currency: string;
  totalDebit: number;
  totalCredit: number;
  isBalanced: boolean;
  lines: TrialBalanceLineDto[];
}

export interface LegacyMigrationResultDto {
  migratedEntries: number;
  previouslyMigratedEntries: number;
  legacySignedTotal: number;
  journalBankTotal: number;
  difference: number;
  isReconciled: boolean;
}

// ── OPERATIONAL DASHBOARD ───────────────────────────────────────────────────

export interface DashboardFilters {
  from?: string;
  to?: string;
  bankAccountId?: string;
  clientId?: string;
  currency?: string;
}

export interface DashboardMetricDto {
  count: number;
  amount?: number | null;
  currency: string;
}

export interface DashboardSummaryDto {
  activeClients?: DashboardMetricDto | null;
  outstandingCharges?: DashboardMetricDto | null;
  overdueCharges?: DashboardMetricDto | null;
  ledgerMovements?: DashboardMetricDto | null;
  from: string;
  to: string;
  currency: string;
}

export interface DashboardAttentionItemDto {
  kind: string;
  severity: "Attention" | "Risk" | "Info";
  title: string;
  detail: string;
  count: number;
  amount?: number | null;
  currency: string;
  href: string;
  permission: string;
}

export interface DashboardAttentionDto {
  items: DashboardAttentionItemDto[];
  generatedAt: string;
}

export interface DashboardTrendPointDto {
  periodStart: string;
  label: string;
  charges: number;
  payments: number;
}

export interface DashboardActivityDto {
  trend: DashboardTrendPointDto[];
  recentMovements: LedgerEntryListDto[];
  summary: string;
  currency: string;
  from: string;
  to: string;
}

export interface BankImportAttemptDto {
  id: string;
  bankAccountName: string;
  fileName: string;
  status: string;
  error: string | null;
  recordsImported: number;
  attemptedAt: string;
}

export interface UnreconciledBankLineDto {
  id: string;
  bankAccountName: string;
  transactionDate: string;
  description: string;
  amount: number;
  currency: string;
  reference: string | null;
}

export interface CurrencyConfigDto {
  id: string;
  code: string;
  name: string;
  symbol: string;
  isActive: boolean;
  isDefault: boolean;
}

export interface CreateCurrencyConfigRequest {
  code: string;
  name: string;
  symbol: string;
  isActive: boolean;
  isDefault: boolean;
}

export type UpdateCurrencyConfigRequest = Omit<CreateCurrencyConfigRequest, "code">;

export interface ExchangeRateDto {
  id: string;
  currencyCode: string;
  rateToMXN: number;
  rateFromMXN: number;
  effectiveDate: string;
  source: string;
}

export interface ExchangeRateRequest {
  currencyCode: string;
  rateToMXN: number;
  rateFromMXN: number;
  effectiveDate: string;
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
  openingDate: string;
  ledgerBalance: number;
  bankBalance?: number | null;
  bankBalanceAsOfDate?: string | null;
  comparisonLedgerBalance?: number | null;
  difference?: number | null;
  isActive: boolean;
}

export interface BankAccountDto extends BankAccountListDto {}

export interface BankAccountBalancesDto {
  bankAccountId: string;
  currency: string;
  asOfDate: string;
  ledgerBalance: number;
  bankBalance?: number | null;
  bankBalanceAsOfDate?: string | null;
  comparisonLedgerBalance?: number | null;
  difference?: number | null;
}

export interface GetBankAccountsParams {
  isActive?: boolean;
  search?: string;
  pageNumber?: number;
  pageSize?: number;
  includeBalances?: boolean;
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

export interface RecordBankBalanceRequest {
  asOfDate: string;
  balance: number;
}

export type BankImportStatus = "Preview" | "Committed" | "RolledBack" | "Failed";
export type BankImportRowStatus = "Valid" | "Duplicate" | "Incomplete" | "Rejected";

export interface BankImportIssueDto { field: string; message: string; correction: string; }
export interface BankImportRowDto {
  id: string;
  rowNumber: number;
  transactionDate?: string | null;
  description?: string | null;
  reference?: string | null;
  amount?: number | null;
  currency?: string | null;
  balance?: number | null;
  status: BankImportRowStatus;
  issues: BankImportIssueDto[];
}
export interface BankImportBatchDto {
  id: string;
  bankAccountId: string;
  bankAccountName: string;
  fileName: string;
  fileHash: string;
  adapterCode: string;
  status: BankImportStatus;
  totalRecords: number;
  validRecords: number;
  duplicateRecords: number;
  incompleteRecords: number;
  rejectedRecords: number;
  recordsImported: number;
  bankStatementId?: string | null;
  attemptedAt: string;
  completedAt?: string | null;
  rows: BankImportRowDto[];
}
export interface BankImportAdapterDto { code: string; name: string; description: string; }
export interface BankImportProfileDto {
  id: string; name: string; adapterCode: string; dateColumn: string; descriptionColumn: string;
  referenceColumn?: string | null; amountColumn?: string | null; debitColumn?: string | null;
  creditColumn?: string | null; currencyColumn?: string | null; balanceColumn?: string | null;
  dateFormat?: string | null; delimiter: string; headerRow: number;
}

export type ReconciliationStatus = "Suggested" | "Confirmed" | "Rejected" | "Reversed";
export type ReconciliationMatchType = "Exact" | "Suggested" | "OneToMany" | "ManyToOne" | "Partial" | "Difference";
export type ReconciliationDifferenceType = "None" | "Commission" | "Refund" | "Transfer" | "Timing" | "Other";
export interface ReconciliationFactorDto { code: string; label: string; points: number; detail: string; }
export interface ReconciliationBankLineDto { id: string; date: string; description: string; reference?: string | null; amount: number; appliedAmount: number; currency: string; }
export interface ReconciliationLedgerEntryDto { id: string; date: string; description: string; reference?: string | null; clientName?: string | null; amount: number; appliedAmount: number; currency: string; }
export interface ReconciliationCaseDto {
  id: string; bankAccountId: string; bankAccountName: string; status: ReconciliationStatus; matchType: ReconciliationMatchType;
  score: number; bankAmount: number; ledgerAmount: number; differenceAmount: number; differenceType: ReconciliationDifferenceType;
  differenceReason?: string | null; isAutomatic: boolean; generatedAt: string; confirmedAt?: string | null; reversedAt?: string | null;
  factors: ReconciliationFactorDto[]; bankLines: ReconciliationBankLineDto[]; ledgerEntries: ReconciliationLedgerEntryDto[];
}
export interface ReconciliationSettingsDto {
  dateToleranceDays: number; amountTolerance: number; suggestionThreshold: number; autoConfirmThreshold: number;
  allowAutoConfirmExact: boolean; amountWeight: number; dateWeight: number; referenceWeight: number; clientWeight: number; patternWeight: number;
}
export interface ReconciliationRunResultDto { suggestionsCreated: number; exactMatches: number; combinedMatches: number; partialMatches: number; autoConfirmed: number; }
export interface ReconciliationPeriodDto {
  id: string; bankAccountId: string; bankAccountName: string; startDate: string; endDate: string; currency: string;
  bankAmount: number; ledgerAmount: number; differenceAmount: number; differenceType: ReconciliationDifferenceType;
  justification?: string | null; status: "Open" | "Closed"; closedAt?: string | null;
}

// ── ACCOUNT STATEMENTS ────────────────────────────────────────────────────────

export interface AccountStatementItemDto {
  id: string;
  journalEntryId: string;
  journalEntryNumber: string;
  date: string;
  type: string;
  sourceType: string;
  description: string;
  reference?: string | null;
  referenceId: string;
  subscriptionId?: string | null;
  subscriptionCode?: string | null;
  serviceName?: string | null;
  dueDate?: string | null;
  isOverdue: boolean;
  debit: number;
  credit: number;
  balance: number;
  currency: string;
}

export interface AccountStatementDto {
  clientId: string;
  clientName: string;
  organizationName: string;
  statementDate: string;
  startDate?: string | null;
  endDate?: string | null;
  displayCurrency: string;
  subscriptionId?: string | null;
  subscriptionCode?: string | null;
  serviceName?: string | null;
  initialBalance: number;
  totalCharges: number;
  totalPayments: number;
  totalAdjustments: number;
  finalBalance: number;
  overdueBalance: number;
  isSubledgerView: boolean;
  scopeDescription: string;
  contactName?: string | null;
  contactEmail?: string | null;
  verificationCode: string;
  formulaValid: boolean;
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
  subscriptionId?: string;
}

export interface AccountStatementInquiryDto {
  id: string; clientId: string; subscriptionId?: string | null; subscriptionCode?: string | null;
  journalEntryId?: string | null; journalEntryNumber?: string | null; statementVerificationCode?: string | null;
  subject: string; message: string; status: "Open" | "InReview" | "Resolved" | "Closed";
  requestedAt: string; resolvedAt?: string | null; resolution?: string | null;
}

export interface AccountStatementClientOptionDto { id: string; code: string; name: string; }
export interface AccountStatementSubscriptionOptionDto { id: string; code: string; serviceName: string; }

export interface CreateAccountStatementInquiryRequest {
  subscriptionId?: string | null; journalEntryId?: string | null; statementVerificationCode?: string | null;
  subject: string; message: string; idempotencyKey: string;
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
  isReversed: boolean;
  paymentApplicationId?: string | null;
  reversedAt?: string | null;
  reversalReason?: string | null;
  billingItemDescription?: string | null;
  ledgerEntryDescription?: string | null;
}

export interface AllocationResult {
  success: boolean;
  allocatedAmount: number;
  remainingAmount: number;
  paymentApplicationId?: string | null;
  receiptNumber?: string | null;
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

export type PaymentApplicationPriority = "DueDate" | "Plan" | "Reference" | "Selection";
export type PaymentApplicationOrigin = "Manual" | "Automatic" | "Reapplication" | "Legacy";
export type PaymentApplicationStatus = "Active" | "Reversed";

export interface PaymentApplicationPaymentDto {
  ledgerEntryId: string;
  description: string;
  reference?: string | null;
  date: string;
  journalEntryId: string;
  journalEntryNumber: string;
  availableBefore: number;
  appliedAmount: number;
  unappliedAfter: number;
}

export interface PaymentApplicationLineDto {
  id: string;
  ledgerEntryId: string;
  billingItemId: string;
  billingItemDescription: string;
  amount: number;
  isReversed: boolean;
}

export interface PaymentApplicationDto {
  id: string;
  clientId: string;
  clientName: string;
  receiptNumber: string;
  priority: PaymentApplicationPriority;
  origin: PaymentApplicationOrigin;
  status: PaymentApplicationStatus;
  totalPaymentAmount: number;
  appliedAmount: number;
  unappliedAmount: number;
  currency: string;
  appliedAt: string;
  appliedBy?: string | null;
  reversedAt?: string | null;
  reversalReason?: string | null;
  reappliesPaymentApplicationId?: string | null;
  payments: PaymentApplicationPaymentDto[];
  allocations: PaymentApplicationLineDto[];
}

export interface PaymentApplicationSettingsDto { defaultPriority: PaymentApplicationPriority; }
export interface AutomaticPaymentApplicationRequest { ledgerEntryIds: string[]; priority?: PaymentApplicationPriority; preferredServiceId?: string | null; idempotencyKey: string; }
export interface PaymentAllocationLineRequest { ledgerEntryId: string; billingItemId: string; amount: number; }
export interface ManualPaymentApplicationRequest { allocations: PaymentAllocationLineRequest[]; idempotencyKey: string; }
