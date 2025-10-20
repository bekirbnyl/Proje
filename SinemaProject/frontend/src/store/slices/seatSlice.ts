import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import { SeatLayout, SeatHold, Seat, CreateSeatHoldsRequest, Hall } from '../../types';
import apiService from '../../services/api';

interface SeatState {
  halls: Hall[];
  seatLayouts: { [hallId: string]: SeatLayout[] };
  activeLayout: SeatLayout | null;
  selectedSeats: string[];
  seatHolds: SeatHold[];
  clientToken: string;
  isLoading: boolean;
  error: string | null;
}

const initialState: SeatState = {
  halls: [],
  seatLayouts: {},
  activeLayout: null,
  selectedSeats: [],
  seatHolds: [],
  clientToken: Math.random().toString(36).substring(2, 15) + Math.random().toString(36).substring(2, 15),
  isLoading: false,
  error: null,
};

// Async thunks
export const fetchHalls = createAsyncThunk(
  'seats/fetchHalls',
  async (_, { rejectWithValue }) => {
    try {
      const halls = await apiService.getHalls();
      return halls;
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Failed to fetch halls');
    }
  }
);

export const fetchHallSeatLayouts = createAsyncThunk(
  'seats/fetchHallSeatLayouts',
  async (hallId: string, { rejectWithValue }) => {
    try {
      const layouts = await apiService.getHallSeatLayouts(hallId);
      return { hallId, layouts };
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Failed to fetch seat layouts');
    }
  }
);

export const fetchActiveHallLayout = createAsyncThunk(
  'seats/fetchActiveHallLayout',
  async (hallId: string, { rejectWithValue }) => {
    try {
      const layout = await apiService.getActiveHallLayout(hallId);
      return layout;
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Failed to fetch active layout');
    }
  }
);

export const createSeatHolds = createAsyncThunk(
  'seats/createSeatHolds',
  async (request: CreateSeatHoldsRequest, { rejectWithValue }) => {
    try {
      const holds = await apiService.createSeatHolds(request);
      return holds;
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Failed to create seat holds');
    }
  }
);

export const extendSeatHold = createAsyncThunk(
  'seats/extendSeatHold',
  async (holdId: string, { rejectWithValue }) => {
    try {
      const hold = await apiService.extendSeatHold(holdId);
      return hold;
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Failed to extend seat hold');
    }
  }
);

export const releaseSeatHold = createAsyncThunk(
  'seats/releaseSeatHold',
  async (holdId: string, { rejectWithValue }) => {
    try {
      await apiService.releaseSeatHold(holdId);
      return holdId;
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Failed to release seat hold');
    }
  }
);

export const addSeatLayoutVersion = createAsyncThunk(
  'seats/addSeatLayoutVersion',
  async ({ hallId, seats }: { hallId: string; seats: any[] }, { rejectWithValue }) => {
    try {
      const layout = await apiService.addSeatLayoutVersion(hallId, seats);
      return { hallId, layout };
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Failed to add seat layout version');
    }
  }
);

export const activateSeatLayout = createAsyncThunk(
  'seats/activateSeatLayout',
  async ({ hallId, layoutId }: { hallId: string; layoutId: string }, { rejectWithValue }) => {
    try {
      await apiService.activateSeatLayout(hallId, layoutId);
      return { hallId, layoutId };
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Failed to activate seat layout');
    }
  }
);

const seatSlice = createSlice({
  name: 'seats',
  initialState,
  reducers: {
    clearError: (state) => {
      state.error = null;
    },
    selectSeat: (state, action: PayloadAction<string>) => {
      const seatId = action.payload;
      if (!state.selectedSeats.includes(seatId)) {
        state.selectedSeats.push(seatId);
      }
    },
    deselectSeat: (state, action: PayloadAction<string>) => {
      const seatId = action.payload;
      state.selectedSeats = state.selectedSeats.filter(id => id !== seatId);
    },
    toggleSeat: (state, action: PayloadAction<string>) => {
      const seatId = action.payload;
      if (state.selectedSeats.includes(seatId)) {
        state.selectedSeats = state.selectedSeats.filter(id => id !== seatId);
      } else {
        state.selectedSeats.push(seatId);
      }
    },
    clearSelectedSeats: (state) => {
      state.selectedSeats = [];
    },
    clearSeatHolds: (state) => {
      state.seatHolds = [];
    },
    generateNewClientToken: (state) => {
      state.clientToken = Math.random().toString(36).substring(2, 15) + Math.random().toString(36).substring(2, 15);
    },
  },
  extraReducers: (builder) => {
    // Fetch halls
    builder
      .addCase(fetchHalls.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchHalls.fulfilled, (state, action) => {
        state.isLoading = false;
        state.halls = action.payload;
        state.error = null;
      })
      .addCase(fetchHalls.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Fetch hall seat layouts
    builder
      .addCase(fetchHallSeatLayouts.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchHallSeatLayouts.fulfilled, (state, action) => {
        state.isLoading = false;
        state.seatLayouts[action.payload.hallId] = action.payload.layouts;
        state.error = null;
      })
      .addCase(fetchHallSeatLayouts.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Fetch active hall layout
    builder
      .addCase(fetchActiveHallLayout.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchActiveHallLayout.fulfilled, (state, action) => {
        state.isLoading = false;
        state.activeLayout = action.payload;
        state.error = null;
      })
      .addCase(fetchActiveHallLayout.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Create seat holds
    builder
      .addCase(createSeatHolds.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(createSeatHolds.fulfilled, (state, action) => {
        state.isLoading = false;
        state.seatHolds = action.payload;
        state.error = null;
      })
      .addCase(createSeatHolds.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Extend seat hold
    builder
      .addCase(extendSeatHold.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(extendSeatHold.fulfilled, (state, action) => {
        state.isLoading = false;
        const index = state.seatHolds.findIndex(hold => hold.holdId === action.payload.holdId);
        if (index !== -1) {
          state.seatHolds[index] = action.payload;
        }
        state.error = null;
      })
      .addCase(extendSeatHold.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Release seat hold
    builder
      .addCase(releaseSeatHold.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(releaseSeatHold.fulfilled, (state, action) => {
        state.isLoading = false;
        const holdId = action.payload;
        state.seatHolds = state.seatHolds.filter(hold => hold.holdId !== holdId);
        state.error = null;
      })
      .addCase(releaseSeatHold.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Add seat layout version
    builder
      .addCase(addSeatLayoutVersion.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(addSeatLayoutVersion.fulfilled, (state, action) => {
        state.isLoading = false;
        const { hallId, layout } = action.payload;
        if (!state.seatLayouts[hallId]) {
          state.seatLayouts[hallId] = [];
        }
        state.seatLayouts[hallId].push(layout);
        state.error = null;
      })
      .addCase(addSeatLayoutVersion.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Activate seat layout
    builder
      .addCase(activateSeatLayout.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(activateSeatLayout.fulfilled, (state, action) => {
        state.isLoading = false;
        const { hallId, layoutId } = action.payload;
        if (state.seatLayouts[hallId]) {
          state.seatLayouts[hallId].forEach(layout => {
            layout.isActive = layout.id === layoutId;
          });
        }
        state.error = null;
      })
      .addCase(activateSeatLayout.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });
  },
});

export const {
  clearError,
  selectSeat,
  deselectSeat,
  toggleSeat,
  clearSelectedSeats,
  clearSeatHolds,
  generateNewClientToken,
} = seatSlice.actions;

export default seatSlice.reducer;
