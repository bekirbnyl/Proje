import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { Screening, ScreeningListItem, CreateScreeningRequest, UpdateScreeningRequest } from '../../types';
import apiService from '../../services/api';

interface ScreeningState {
  screenings: ScreeningListItem[];
  currentScreening: Screening | null;
  isLoading: boolean;
  error: string | null;
}

const initialState: ScreeningState = {
  screenings: [],
  currentScreening: null,
  isLoading: false,
  error: null,
};

// Async thunks
export const fetchScreenings = createAsyncThunk(
  'screenings/fetchScreenings',
  async (params: { date?: Date; hallId?: string; movieId?: string } = {}, { rejectWithValue }) => {
    try {
      const screenings = await apiService.getScreenings(params.date, params.hallId, params.movieId);
      return screenings;
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Failed to fetch screenings');
    }
  }
);

export const fetchScreening = createAsyncThunk(
  'screenings/fetchScreening',
  async (id: string, { rejectWithValue }) => {
    try {
      const screening = await apiService.getScreening(id);
      return screening;
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Failed to fetch screening');
    }
  }
);

export const createScreening = createAsyncThunk(
  'screenings/createScreening',
  async (screeningData: CreateScreeningRequest, { rejectWithValue }) => {
    try {
      const screening = await apiService.createScreening(screeningData);
      return screening;
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Failed to create screening');
    }
  }
);

export const updateScreening = createAsyncThunk(
  'screenings/updateScreening',
  async ({ id, screeningData }: { id: string; screeningData: UpdateScreeningRequest }, { rejectWithValue }) => {
    try {
      const screening = await apiService.updateScreening(id, screeningData);
      return screening;
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Failed to update screening');
    }
  }
);

export const deleteScreening = createAsyncThunk(
  'screenings/deleteScreening',
  async ({ id, reason }: { id: string; reason: string }, { rejectWithValue }) => {
    try {
      await apiService.deleteScreening(id, reason);
      return id;
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Failed to delete screening');
    }
  }
);

const screeningSlice = createSlice({
  name: 'screenings',
  initialState,
  reducers: {
    clearError: (state) => {
      state.error = null;
    },
    clearCurrentScreening: (state) => {
      state.currentScreening = null;
    },
  },
  extraReducers: (builder) => {
    // Fetch screenings
    builder
      .addCase(fetchScreenings.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchScreenings.fulfilled, (state, action) => {
        state.isLoading = false;
        state.screenings = action.payload;
        state.error = null;
      })
      .addCase(fetchScreenings.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Fetch screening
    builder
      .addCase(fetchScreening.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchScreening.fulfilled, (state, action) => {
        state.isLoading = false;
        state.currentScreening = action.payload;
        state.error = null;
      })
      .addCase(fetchScreening.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Create screening
    builder
      .addCase(createScreening.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(createScreening.fulfilled, (state, action) => {
        state.isLoading = false;
        state.error = null;
      })
      .addCase(createScreening.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Update screening
    builder
      .addCase(updateScreening.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(updateScreening.fulfilled, (state, action) => {
        state.isLoading = false;
        if (state.currentScreening?.id === action.payload.id) {
          state.currentScreening = action.payload;
        }
        state.error = null;
      })
      .addCase(updateScreening.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Delete screening
    builder
      .addCase(deleteScreening.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(deleteScreening.fulfilled, (state, action) => {
        state.isLoading = false;
        const screeningId = action.payload;
        state.screenings = state.screenings.filter(screening => screening.id !== screeningId);
        if (state.currentScreening?.id === screeningId) {
          state.currentScreening = null;
        }
        state.error = null;
      })
      .addCase(deleteScreening.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });
  },
});

export const { clearError, clearCurrentScreening } = screeningSlice.actions;
export default screeningSlice.reducer;
