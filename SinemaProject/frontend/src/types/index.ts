// Base types for the application

export interface Movie {
  id: string;
  title: string;
  durationMinutes: number;
  isActive: boolean;
  createdAt: string;
  isDeleted: boolean;
  deletedAt?: string;
  deletedBy?: string;
}

export interface Screening {
  id: string;
  movieId: string;
  hallId: string;
  startAt: string;
  durationMinutes: number;
  seatLayoutId: string;
  movie?: Movie;
  hall?: Hall;
}

export interface ScreeningListItem {
  id: string;
  movieId: string;
  hallId: string;
  startAt: string;
  durationMinutes: number;
  movieTitle: string;
  hallName: string;
  cinemaName: string;
}

export interface Hall {
  id: string;
  name: string;
  cinemaId: string;
}

export interface Seat {
  id: string;
  seatLayoutId: string;
  row: number;
  col: number;
  label: string;
  isActive?: boolean;
}

export interface SeatLayout {
  id: string;
  hallId: string;
  version: number;
  isActive: boolean;
  createdAt: string;
  seats: Seat[];
}

export interface SeatHold {
  holdId: string;
  seatId: string;
  screeningId: string;
  expiresAt: string;
  clientToken: string;
  createdAt: string;
  lastHeartbeatAt: string;
  seat?: {
    row: number;
    col: number;
    label: string;
  };
}

export interface User {
  userId: string;
  email: string;
  displayName: string;
  roles: string[];
  memberId?: string;
  isApproved: boolean;
  isVip: boolean;
}

export interface Member {
  id: string;
  fullName: string;
  email: string;
  phoneNumber?: string;
  userId?: string;
  vipStatus: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface MemberApproval {
  id: string;
  memberId: string;
  approved: boolean;
  reason: string;
  approvedBy?: string;
  createdAt: string;
  member?: Member;
}

export interface Reservation {
  id: string;
  screeningId: string;
  seatId: string;
  memberId: string;
  status: ReservationStatus;
  expiresAt: string;
  confirmedAt?: string;
  canceledAt?: string;
  createdAt: string;
}

export interface Ticket {
  id: string;
  screeningId: string;
  seatId: string;
  reservationId?: string;
  price: number;
  channel: TicketChannel;
  type: TicketType;
  soldAt: string;
  soldBy?: string;
  voidedAt?: string;
  voidedBy?: string;
  voidReason?: string;
}

// Enums
export enum ReservationStatus {
  Pending = 0,
  Confirmed = 1,
  Expired = 2,
  Canceled = 3,
  Completed = 4
}

export enum TicketChannel {
  Online = 0,
  BoxOffice = 1,
  Mobile = 2
}

export enum TicketType {
  Full = 0,
  Student = 1,
  Child = 2
}

export enum PaymentMethod {
  Cash = 0,
  CreditCard = 1,
  DebitCard = 2,
  MemberCredit = 3,
  BankTransfer = 4
}

export enum PaymentStatus {
  Pending = 0,
  Succeeded = 1,
  Failed = 2,
  Refunded = 3
}

// Auth types
export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  displayName?: string;
  phoneNumber?: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  tokenType: string;
  memberStatus?: string;
}

// API Request/Response types
export interface CreateMovieRequest {
  title: string;
  durationMinutes: number;
  isActive: boolean;
}

export interface UpdateMovieRequest {
  title: string;
  durationMinutes: number;
  isActive: boolean;
}

export interface CreateScreeningRequest {
  movieId: string;
  hallId: string;
  startAt: string;
  durationMinutes: number;
  seatLayoutId: string;
}

export interface UpdateScreeningRequest {
  movieId: string;
  hallId: string;
  startAt: string;
  durationMinutes: number;
  seatLayoutId: string;
}

export interface CreateSeatHoldsRequest {
  screeningId: string;
  seatIds: string[];
  clientToken: string;
  ttlSeconds?: number;
}

export interface CreateReservationRequest {
  screeningId: string;
  seatIds: string[];
  clientToken: string;
}

export interface SellTicketRequest {
  screeningId: string;
  reservationId?: string;
  memberId?: string;
  channel: TicketChannel;
  paymentMethod: PaymentMethod;
  clientToken?: string;
  items?: SellTicketItemRequest[];
  idempotencyKey?: string;
}

export interface SellTicketItemRequest {
  seatId: string;
  ticketType: TicketType;
  isVipGuest: boolean;
}

export interface TicketInfo {
  seatId: string;
  type: TicketType;
  price: number;
}

export interface SellTicketResponse {
  paymentStatus: PaymentStatus;
  totalBefore: number;
  totalAfter: number;
  items: TicketItemResponse[];
  paymentReference?: string;
}

export interface TicketItemResponse {
  ticketId: string;
  ticketCode: string;
  seatId: string;
  finalPrice: number;
  appliedRule?: AppliedRuleDto;
}

export interface AppliedRuleDto {
  code: string;
  title: string;
  percentOff: number;
  amountOff: number;
  finalPrice: number;
  details?: string;
}

// Pricing types
export interface PriceQuoteRequest {
  screeningId: string;
  memberId?: string;
  items: QuoteItemRequest[];
}

export interface QuoteItemRequest {
  ticketType: TicketType;
  quantity: number;
}

export interface PriceQuoteResponse {
  screeningId: string;
  memberId?: string;
  items: QuoteItemResponse[];
  subtotal: number;
  discountAmount: number;
  totalBefore: number;
  totalAfter: number;
  currency: string;
  appliedDiscounts: AppliedDiscount[];
  validUntil: string;
  messages: string[];
}

export interface QuoteItemResponse {
  ticketType: TicketType;
  quantity: number;
  unitPrice: number;
  baseAmount: number;
  discountAmount: number;
  finalAmount: number;
  appliedDiscounts: string[];
}

export interface AppliedDiscount {
  ruleName: string;
  description: string;
  discountAmount: number;
  discountType: 'Percentage' | 'Fixed';
  originalValue: number;
}

// Settings types
export interface SettingDto {
  key: string;
  value: string;
  dataType: string;
  description: string;
  category: string;
  isReadOnly: boolean;
  updatedAt?: string;
  updatedBy?: string;
  rowVersion: string;
}

export interface UpdateSettingsRequest {
  items: SettingUpdateItem[];
}

export interface SettingUpdateItem {
  key: string;
  value: string;
}

// User roles
export const USER_ROLES = {
  ADMIN: 'Admin',
  PATRON: 'Patron',
  YONETIM: 'Yonetim',
  DEPARTMAN_MUDURU: 'DepartmanMuduru',
  SINEMA_MUDURU: 'SinemaMuduru',
  GISE_AMIRI: 'GiseAmiri',
  GISE_GOREVLISI: 'GiseGorevlisi',
  WEB_UYE: 'WebUye',
  APPROVED_MEMBER: 'ApprovedMember',
  VIP_MEMBER: 'VipMember'
} as const;

export type UserRole = typeof USER_ROLES[keyof typeof USER_ROLES];

// Error types
export interface ApiError {
  title: string;
  detail: string;
  status: number;
  instance?: string;
}
