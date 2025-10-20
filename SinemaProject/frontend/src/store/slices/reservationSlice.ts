import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { Reservation, CreateReservationRequest } from '../../types';
import apiService from '../../services/api';

interface ReservationState {
  reservations: Reservation[];
  current: Reservation | null;
  isLoading: boolean;
  error: string | null;
}

const initialState: ReservationState = {
  reservations: [],
  current: null,
  isLoading: false,
  error: null,
};

export const createReservation = createAsyncThunk(
  'reservations/createReservation',
  async (request: CreateReservationRequest, { rejectWithValue }) => {
    try {
      const reservations = await apiService.createReservation(request);
      return reservations;
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Failed to create reservation');
    }
  }
);

const reservationSlice = createSlice({
  name: 'reservations',
  initialState,
  reducers: {
    clearError: (state) => {
      state.error = null;
    },
    clearCurrentReservation: (state) => {
      state.current = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(createReservation.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(createReservation.fulfilled, (state, action) => {
        state.isLoading = false;
        state.reservations = action.payload;
        state.current = action.payload[0] ?? null;
        state.error = null;
      })
      .addCase(createReservation.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });
  },
});

export const { clearError, clearCurrentReservation } = reservationSlice.actions;
export default reservationSlice.reducer;
