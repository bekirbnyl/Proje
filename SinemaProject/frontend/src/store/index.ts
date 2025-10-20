import { configureStore } from '@reduxjs/toolkit';
import authSlice from './slices/authSlice';
import movieSlice from './slices/movieSlice';
import screeningSlice from './slices/screeningSlice';
import seatSlice from './slices/seatSlice';
import adminSlice from './slices/adminSlice';
import uiSlice from './slices/uiSlice';
import reservationSlice from './slices/reservationSlice';

export const store = configureStore({
  reducer: {
    auth: authSlice,
    movies: movieSlice,
    screenings: screeningSlice,
    seats: seatSlice,
    admin: adminSlice,
    ui: uiSlice,
    reservations: reservationSlice,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: {
        ignoredActions: ['persist/PERSIST'],
      },
    }),
  devTools: process.env.NODE_ENV !== 'production',
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
