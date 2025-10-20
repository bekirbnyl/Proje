import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import { Movie, CreateMovieRequest, UpdateMovieRequest } from '../../types';
import apiService from '../../services/api';

interface MovieState {
  movies: Movie[];
  currentMovie: Movie | null;
  isLoading: boolean;
  error: string | null;
}

const initialState: MovieState = {
  movies: [],
  currentMovie: null,
  isLoading: false,
  error: null,
};

// Async thunks
export const fetchMovies = createAsyncThunk(
  'movies/fetchMovies',
  async (params: { active?: boolean; includeDeleted?: boolean } = {}, { rejectWithValue }) => {
    try {
      const movies = await apiService.getMovies(params.active, params.includeDeleted);
      return movies;
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Failed to fetch movies');
    }
  }
);

export const fetchMovie = createAsyncThunk(
  'movies/fetchMovie',
  async (id: string, { rejectWithValue }) => {
    try {
      const movie = await apiService.getMovie(id);
      return movie;
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Failed to fetch movie');
    }
  }
);

export const createMovie = createAsyncThunk(
  'movies/createMovie',
  async (movieData: CreateMovieRequest, { rejectWithValue }) => {
    try {
      const movie = await apiService.createMovie(movieData);
      return movie;
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Failed to create movie');
    }
  }
);

export const updateMovie = createAsyncThunk(
  'movies/updateMovie',
  async ({ id, movieData }: { id: string; movieData: UpdateMovieRequest }, { rejectWithValue }) => {
    try {
      const movie = await apiService.updateMovie(id, movieData);
      return movie;
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Failed to update movie');
    }
  }
);

export const deleteMovie = createAsyncThunk(
  'movies/deleteMovie',
  async ({ id, reason }: { id: string; reason: string }, { rejectWithValue }) => {
    try {
      await apiService.deleteMovie(id, reason);
      return id;
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Failed to delete movie');
    }
  }
);

export const restoreMovie = createAsyncThunk(
  'movies/restoreMovie',
  async (id: string, { rejectWithValue }) => {
    try {
      await apiService.restoreMovie(id);
      return id;
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Failed to restore movie');
    }
  }
);

const movieSlice = createSlice({
  name: 'movies',
  initialState,
  reducers: {
    clearError: (state) => {
      state.error = null;
    },
    clearCurrentMovie: (state) => {
      state.currentMovie = null;
    },
  },
  extraReducers: (builder) => {
    // Fetch movies
    builder
      .addCase(fetchMovies.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchMovies.fulfilled, (state, action) => {
        state.isLoading = false;
        state.movies = action.payload;
        state.error = null;
      })
      .addCase(fetchMovies.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Fetch movie
    builder
      .addCase(fetchMovie.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchMovie.fulfilled, (state, action) => {
        state.isLoading = false;
        state.currentMovie = action.payload;
        state.error = null;
      })
      .addCase(fetchMovie.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Create movie
    builder
      .addCase(createMovie.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(createMovie.fulfilled, (state, action) => {
        state.isLoading = false;
        state.movies.push(action.payload);
        state.error = null;
      })
      .addCase(createMovie.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Update movie
    builder
      .addCase(updateMovie.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(updateMovie.fulfilled, (state, action) => {
        state.isLoading = false;
        const index = state.movies.findIndex(movie => movie.id === action.payload.id);
        if (index !== -1) {
          state.movies[index] = action.payload;
        }
        if (state.currentMovie?.id === action.payload.id) {
          state.currentMovie = action.payload;
        }
        state.error = null;
      })
      .addCase(updateMovie.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Delete movie
    builder
      .addCase(deleteMovie.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(deleteMovie.fulfilled, (state, action) => {
        state.isLoading = false;
        const movieId = action.payload;
        state.movies = state.movies.filter(movie => movie.id !== movieId);
        if (state.currentMovie?.id === movieId) {
          state.currentMovie = null;
        }
        state.error = null;
      })
      .addCase(deleteMovie.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Restore movie
    builder
      .addCase(restoreMovie.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(restoreMovie.fulfilled, (state, action) => {
        state.isLoading = false;
        state.error = null;
      })
      .addCase(restoreMovie.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });
  },
});

export const { clearError, clearCurrentMovie } = movieSlice.actions;
export default movieSlice.reducer;
