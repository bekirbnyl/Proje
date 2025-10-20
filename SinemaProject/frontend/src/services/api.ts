import axios, { type AxiosInstance, type AxiosResponse, AxiosError } from 'axios';
import type { 
  AuthResponse, 
  LoginRequest, 
  RegisterRequest, 
  User,
  Movie,
  Screening,
  ScreeningListItem,
  Hall,
  SeatLayout,
  SeatHold,
  CreateMovieRequest,
  UpdateMovieRequest,
  CreateScreeningRequest,
  UpdateScreeningRequest,
  CreateSeatHoldsRequest,
  CreateReservationRequest,
  SellTicketRequest,
  SellTicketResponse,
  Reservation,
  Ticket,
  MemberApproval,
  ApiError,
  PriceQuoteRequest,
  PriceQuoteResponse,
  QuoteItemResponse,
  TicketType,
  SettingDto,
  UpdateSettingsRequest
} from '../types';

class ApiService {
  private api: AxiosInstance;
  private refreshTokenPromise: Promise<string> | null = null;

  constructor() {
    this.api = axios.create({
      baseURL: 'http://localhost:5238/api',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Request interceptor to add auth token
    this.api.interceptors.request.use(
      (config) => {
        const token = localStorage.getItem('accessToken');
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );

    // Response interceptor to handle token refresh
    this.api.interceptors.response.use(
      (response) => response,
      async (error: AxiosError) => {
        const originalRequest = error.config;
        
        if (error.response?.status === 401 && originalRequest && !originalRequest._retry) {
          originalRequest._retry = true;
          
          try {
            const newToken = await this.refreshToken();
            originalRequest.headers.Authorization = `Bearer ${newToken}`;
            return this.api(originalRequest);
          } catch (refreshError) {
            this.logout();
            window.location.href = '/login';
            return Promise.reject(refreshError);
          }
        }
        
        return Promise.reject(error);
      }
    );
  }

  // Helper method to handle API errors
  private handleApiError(error: AxiosError): never {
    if (error.response?.data) {
      const apiError = error.response.data as ApiError;
      throw new Error(apiError.detail || apiError.title || 'An error occurred');
    }
    throw new Error(error.message || 'Network error');
  }

  // Auth endpoints
  async login(credentials: LoginRequest): Promise<AuthResponse> {
    try {
      const response: AxiosResponse<AuthResponse> = await this.api.post('/auth/login', credentials);
      const { accessToken, refreshToken } = response.data;
      
      localStorage.setItem('accessToken', accessToken);
      localStorage.setItem('refreshToken', refreshToken);
      
      return response.data;
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  async register(userData: RegisterRequest): Promise<AuthResponse> {
    try {
      const response: AxiosResponse<AuthResponse> = await this.api.post('/auth/register', userData);
      const { accessToken, refreshToken } = response.data;
      
      localStorage.setItem('accessToken', accessToken);
      localStorage.setItem('refreshToken', refreshToken);
      
      return response.data;
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  async refreshToken(): Promise<string> {
    if (this.refreshTokenPromise) {
      return this.refreshTokenPromise;
    }

    this.refreshTokenPromise = (async () => {
      try {
        const refreshToken = localStorage.getItem('refreshToken');
        if (!refreshToken) {
          throw new Error('No refresh token available');
        }

        const response: AxiosResponse<AuthResponse> = await this.api.post('/auth/refresh', {
          refreshToken
        });

        const { accessToken, refreshToken: newRefreshToken } = response.data;
        localStorage.setItem('accessToken', accessToken);
        localStorage.setItem('refreshToken', newRefreshToken);

        return accessToken;
      } finally {
        this.refreshTokenPromise = null;
      }
    })();

    return this.refreshTokenPromise;
  }

  async getCurrentUser(): Promise<User> {
    try {
      const response: AxiosResponse<User> = await this.api.get('/auth/me');
      return response.data;
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  logout(): void {
    const refreshToken = localStorage.getItem('refreshToken');
    if (refreshToken) {
      // Fire and forget logout request
      this.api.post('/auth/logout', { refreshToken }).catch(() => {
        // Ignore errors during logout
      });
    }
    
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
  }

  // Movie endpoints
  async getMovies(active?: boolean, includeDeleted = false): Promise<Movie[]> {
    try {
      const params = new URLSearchParams();
      if (active !== undefined) params.append('active', active.toString());
      if (includeDeleted) params.append('includeDeleted', 'true');
      
      const response: AxiosResponse<Movie[]> = await this.api.get(`/v1/movies?${params}`);
      return response.data;
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  async getMovie(id: string): Promise<Movie> {
    try {
      const response: AxiosResponse<Movie> = await this.api.get(`/v1/movies/${id}`);
      return response.data;
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  async createMovie(movieData: CreateMovieRequest): Promise<Movie> {
    try {
      const response: AxiosResponse<Movie> = await this.api.post('/v1/movies', movieData);
      return response.data;
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  async updateMovie(id: string, movieData: UpdateMovieRequest): Promise<Movie> {
    try {
      const response: AxiosResponse<Movie> = await this.api.put(`/v1/movies/${id}`, movieData);
      return response.data;
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  async deleteMovie(id: string, reason: string): Promise<void> {
    try {
      await this.api.delete(`/v1/movies/${id}?reason=${encodeURIComponent(reason)}`);
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  async restoreMovie(id: string): Promise<void> {
    try {
      await this.api.post(`/v1/movies/${id}/restore`);
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  // Screening endpoints
  async getScreenings(date?: Date, hallId?: string, movieId?: string): Promise<ScreeningListItem[]> {
    try {
      const params = new URLSearchParams();
      if (date) params.append('date', date.toISOString().split('T')[0]);
      if (hallId) params.append('hallId', hallId);
      if (movieId) params.append('movieId', movieId);
      
      const response: AxiosResponse<ScreeningListItem[]> = await this.api.get(`/v1/screenings?${params}`);
      return response.data;
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  async getScreening(id: string): Promise<Screening> {
    try {
      const response: AxiosResponse<Screening> = await this.api.get(`/v1/screenings/${id}`);
      return response.data;
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  async createScreening(screeningData: CreateScreeningRequest): Promise<Screening> {
    try {
      const response: AxiosResponse<Screening> = await this.api.post('/v1/screenings', screeningData);
      return response.data;
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  async updateScreening(id: string, screeningData: UpdateScreeningRequest): Promise<Screening> {
    try {
      const response: AxiosResponse<Screening> = await this.api.put(`/v1/screenings/${id}`, screeningData);
      return response.data;
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  async deleteScreening(id: string, reason: string): Promise<void> {
    try {
      await this.api.delete(`/v1/screenings/${id}?reason=${encodeURIComponent(reason)}`);
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  // Hall endpoints
  async getHalls(): Promise<Hall[]> {
    try {
      const response: AxiosResponse<Hall[]> = await this.api.get('/v1/halls');
      return response.data;
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  async getHallSeatLayouts(hallId: string): Promise<SeatLayout[]> {
    try {
      const response: AxiosResponse<SeatLayout[]> = await this.api.get(`/v1/halls/${hallId}/layouts`);
      return response.data;
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  async getActiveHallLayout(hallId: string): Promise<SeatLayout> {
    try {
      const response: AxiosResponse<SeatLayout> = await this.api.get(`/v1/halls/${hallId}/layout?version=active`);
      return response.data;
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  async addSeatLayoutVersion(hallId: string, seats: any[]): Promise<SeatLayout> {
    try {
      const response: AxiosResponse<SeatLayout> = await this.api.post(`/v1/halls/${hallId}/layouts`, { seats });
      return response.data;
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  async activateSeatLayout(hallId: string, layoutId: string): Promise<void> {
    try {
      await this.api.post(`/v1/halls/${hallId}/layouts/${layoutId}/activate`);
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  // Seat hold endpoints
  async createSeatHolds(request: CreateSeatHoldsRequest): Promise<SeatHold[]> {
    try {
      const response: AxiosResponse<SeatHold[]> = await this.api.post('/v1/holds', request);
      return response.data;
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  async extendSeatHold(holdId: string): Promise<SeatHold> {
    try {
      const response: AxiosResponse<SeatHold> = await this.api.post(`/v1/holds/${holdId}/extend`);
      return response.data;
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  async releaseSeatHold(holdId: string): Promise<void> {
    try {
      await this.api.delete(`/v1/holds/${holdId}`);
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  // Reservation endpoints
  async createReservation(request: CreateReservationRequest): Promise<Reservation[]> {
    try {
      const response: AxiosResponse<Reservation[]> = await this.api.post('/v1/reservations', request);
      return response.data;
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  async confirmReservation(reservationId: string): Promise<Reservation> {
    try {
      const response: AxiosResponse<Reservation> = await this.api.post(`/v1/reservations/${reservationId}/confirm`);
      return response.data;
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  async cancelReservation(reservationId: string, reason?: string): Promise<void> {
    try {
      const params = reason ? `?reason=${encodeURIComponent(reason)}` : '';
      await this.api.post(`/v1/reservations/${reservationId}/cancel${params}`);
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  async getReservation(reservationId: string): Promise<Reservation> {
    try {
      const response: AxiosResponse<Reservation> = await this.api.get(`/v1/reservations/${reservationId}`);
      return response.data;
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  // Ticket endpoints
  async sellTickets(request: SellTicketRequest): Promise<SellTicketResponse> {
    try {
      const response: AxiosResponse<SellTicketResponse> = await this.api.post('/v1/tickets', request);
      return response.data;
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  async getTicket(ticketId: string): Promise<Ticket> {
    try {
      const response: AxiosResponse<Ticket> = await this.api.get(`/v1/tickets/${ticketId}`);
      return response.data;
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  async voidTicket(ticketId: string, reason: string): Promise<void> {
    try {
      await this.api.post(`/v1/tickets/${ticketId}/void?reason=${encodeURIComponent(reason)}`);
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  // Member management endpoints
  async getPendingMemberApprovals(): Promise<MemberApproval[]> {
    try {
      const response: AxiosResponse<MemberApproval[]> = await this.api.get('/v1/members/pending-approvals');
      return response.data;
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  async approveMember(memberId: string, reason: string): Promise<void> {
    try {
      await this.api.post(`/v1/members/${memberId}/approve`, { reason });
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  async rejectMember(memberId: string, reason: string): Promise<void> {
    try {
      await this.api.post(`/v1/members/${memberId}/reject`, { reason });
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  async approveVipApplication(approvalId: string, reason: string): Promise<void> {
    try {
      await this.api.post(`/v1/members/approvals/${approvalId}/approve-vip`, { reason });
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  async grantVipStatus(memberId: string): Promise<void> {
    try {
      await this.api.post(`/v1/members/${memberId}/grant-vip`);
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  async revokeVipStatus(memberId: string): Promise<void> {
    try {
      await this.api.post(`/v1/members/${memberId}/revoke-vip`);
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  // Pricing endpoints
  async getPriceQuote(request: PriceQuoteRequest): Promise<PriceQuoteResponse> {
    try {
      const response: AxiosResponse<PriceQuoteResponse> = await this.api.post('/v1/pricing/quote', request);
      return response.data;
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  async getBasePrice(screeningId: string): Promise<number> {
    try {
      const response: AxiosResponse<number> = await this.api.get(`/v1/pricing/base-price/${screeningId}`);
      return response.data;
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  async getQuickQuote(screeningId: string, ticketType?: TicketType, memberId?: string): Promise<QuoteItemResponse> {
    try {
      const params = new URLSearchParams();
      if (ticketType !== undefined) params.append('ticketType', ticketType.toString());
      if (memberId) params.append('memberId', memberId);
      
      const response: AxiosResponse<QuoteItemResponse> = await this.api.get(`/v1/pricing/quick-quote/${screeningId}?${params}`);
      return response.data;
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  // Settings endpoints
  async getSettings(keys?: string[]): Promise<Record<string, SettingDto>> {
    try {
      const params = keys?.length ? `?${keys.map(k => `keys=${k}`).join('&')}` : '';
      const response: AxiosResponse<Record<string, SettingDto>> = await this.api.get(`/v1/settings${params}`);
      return response.data;
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  async updateSettings(settings: UpdateSettingsRequest): Promise<Record<string, SettingDto>> {
    try {
      const response: AxiosResponse<Record<string, SettingDto>> = await this.api.put('/v1/settings', settings);
      return response.data;
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  async patchSetting(key: string, value: string, rowVersion?: string): Promise<SettingDto> {
    try {
      const response: AxiosResponse<SettingDto> = await this.api.patch(`/v1/settings/${key}`, {
        value,
        rowVersion
      });
      return response.data;
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  // Report endpoints
  async downloadSalesReport(params: any): Promise<Blob> {
    try {
      const response = await this.api.get('/v1/reports/sales', {
        params: { ...params, format: 'xlsx' },
        responseType: 'blob'
      });
      return response.data;
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  async downloadDeletionsReport(params: any): Promise<Blob> {
    try {
      const response = await this.api.get('/v1/reports/deletions', {
        params: { ...params, format: 'xlsx' },
        responseType: 'blob'
      });
      return response.data;
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  async downloadMembershipsReport(params: any): Promise<Blob> {
    try {
      const response = await this.api.get('/v1/reports/memberships', {
        params: { ...params, format: 'xlsx' },
        responseType: 'blob'
      });
      return response.data;
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  // VIP application endpoint
  async applyForVip(reason: string): Promise<void> {
    try {
      await this.api.post('/v1/members/vip-application', { reason });
    } catch (error) {
      this.handleApiError(error as AxiosError);
    }
  }

  // Helper method to download blob as file
  downloadBlob(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
  }
}

// Create a singleton instance
const apiService = new ApiService();
export default apiService;

// Extend AxiosRequestConfig to include _retry property
declare module 'axios' {
  interface AxiosRequestConfig {
    _retry?: boolean;
  }
}
