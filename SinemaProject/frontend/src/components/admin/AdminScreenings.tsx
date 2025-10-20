import React, { useEffect, useMemo, useState } from 'react';
import { useAppDispatch, useAppSelector } from '../../store/hooks';
import { RootState } from '../../store';
import { fetchMovies } from '../../store/slices/movieSlice';
import { fetchHalls, fetchActiveHallLayout } from '../../store/slices/seatSlice';
import { fetchScreenings, createScreening, deleteScreening } from '../../store/slices/screeningSlice';

const AdminScreenings: React.FC = () => {
  const dispatch = useAppDispatch();
  const { movies } = useAppSelector((s: RootState) => s.movies);
  const { halls, activeLayout } = useAppSelector((s: RootState) => s.seats);
  const { screenings, isLoading, error } = useAppSelector((s: RootState) => s.screenings);

  const [date, setDate] = useState<string>(new Date().toISOString().split('T')[0]);
  const [selectedHall, setSelectedHall] = useState<string>('');
  const [selectedMovie, setSelectedMovie] = useState<string>('');
  const [startTime, setStartTime] = useState<string>('18:00');
  const [duration, setDuration] = useState<number>(120);

  useEffect(() => {
    dispatch(fetchMovies({ active: true }));
    dispatch(fetchHalls());
  }, [dispatch]);

  useEffect(() => {
    const d = new Date(date);
    dispatch(fetchScreenings({ date: d, hallId: selectedHall || undefined, movieId: selectedMovie || undefined }));
  }, [dispatch, date, selectedHall, selectedMovie]);

  useEffect(() => {
    if (selectedHall) {
      dispatch(fetchActiveHallLayout(selectedHall));
    }
  }, [dispatch, selectedHall]);

  const create = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedHall || !selectedMovie) return;
    const startAt = new Date(`${date}T${startTime}:00`);
    const seatLayoutId = activeLayout?.id || '';
    await dispatch(createScreening({ movieId: selectedMovie, hallId: selectedHall, startAt: startAt.toISOString(), durationMinutes: duration, seatLayoutId }));
    const d = new Date(date);
    dispatch(fetchScreenings({ date: d, hallId: selectedHall || undefined, movieId: selectedMovie || undefined }));
  };

  const remove = async (id: string) => {
    await dispatch(deleteScreening({ id, reason: 'Schedule change' }));
  };

  const screeningsForDay = useMemo(() => screenings, [screenings]);

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-gray-900">Manage Screenings</h1>
        <p className="mt-1 text-sm text-gray-500">Schedule and manage movie screenings</p>
      </div>

      {/* Filters */}
      <div className="card">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <div>
            <label className="form-label" htmlFor="date">Date</label>
            <input id="date" type="date" className="input-field" value={date} onChange={(e) => setDate(e.target.value)} />
          </div>
          <div>
            <label className="form-label" htmlFor="hall">Hall</label>
            <select id="hall" className="input-field" value={selectedHall} onChange={(e) => setSelectedHall(e.target.value)}>
              <option value="">All Halls</option>
              {halls.map(h => <option key={h.id} value={h.id}>{h.name}</option>)}
            </select>
          </div>
          <div>
            <label className="form-label" htmlFor="movie">Movie</label>
            <select id="movie" className="input-field" value={selectedMovie} onChange={(e) => setSelectedMovie(e.target.value)}>
              <option value="">All Movies</option>
              {movies.map(m => <option key={m.id} value={m.id}>{m.title}</option>)}
            </select>
          </div>
        </div>
      </div>

      {/* Create */}
      <div className="card">
        <form onSubmit={create} className="grid grid-cols-1 md:grid-cols-6 gap-4 items-end">
          <div>
            <label className="form-label">Hall</label>
            <select className="input-field" required value={selectedHall} onChange={(e) => setSelectedHall(e.target.value)}>
              <option value="">Select hall</option>
              {halls.map(h => <option key={h.id} value={h.id}>{h.name}</option>)}
            </select>
          </div>
          <div className="md:col-span-2">
            <label className="form-label">Movie</label>
            <select className="input-field" required value={selectedMovie} onChange={(e) => setSelectedMovie(e.target.value)}>
              <option value="">Select movie</option>
              {movies.map(m => <option key={m.id} value={m.id}>{m.title}</option>)}
            </select>
          </div>
          <div>
            <label className="form-label">Date</label>
            <input type="date" className="input-field" required value={date} onChange={(e) => setDate(e.target.value)} />
          </div>
          <div>
            <label className="form-label">Start Time</label>
            <input type="time" className="input-field" required value={startTime} onChange={(e) => setStartTime(e.target.value)} />
          </div>
          <div>
            <label className="form-label">Duration (min)</label>
            <input type="number" min={1} className="input-field" required value={duration} onChange={(e) => setDuration(Number(e.target.value))} />
          </div>
          <div className="md:col-span-6 flex justify-end">
            <button className="btn-primary" type="submit" disabled={!activeLayout}>Create Screening</button>
          </div>
          {activeLayout ? (
            <p className="md:col-span-6 text-xs text-gray-500">Active seat layout selected: <span className="font-semibold">{activeLayout.id}</span></p>
          ) : selectedHall ? (
            <p className="md:col-span-6 text-xs text-yellow-700">No active seat layout found for this hall. Please activate one in Halls.</p>
          ) : null}
        </form>
        {error && <p className="mt-3 text-sm text-red-600">{error}</p>}
      </div>

      {/* List */}
      <div className="card">
        {isLoading ? (
          <div className="flex justify-center py-8">
            <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600"></div>
          </div>
        ) : screeningsForDay.length === 0 ? (
          <div className="text-center text-gray-600">No screenings for selected filters.</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead>
                <tr className="text-left text-xs font-semibold text-gray-600">
                  <th className="px-4 py-3">Movie</th>
                  <th className="px-4 py-3">Hall</th>
                  <th className="px-4 py-3">Start</th>
                  <th className="px-4 py-3">Duration</th>
                  <th className="px-4 py-3 text-right">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100 text-sm">
                {screeningsForDay.map(s => (
                  <tr key={s.id}>
                    <td className="px-4 py-3 font-medium text-gray-900">{s.movieTitle}</td>
                    <td className="px-4 py-3">{s.hallName}</td>
                    <td className="px-4 py-3">{new Date(s.startAt).toLocaleString()}</td>
                    <td className="px-4 py-3">{s.durationMinutes} min</td>
                    <td className="px-4 py-3">
                      <div className="flex justify-end gap-2">
                        <button className="btn-danger" onClick={() => remove(s.id)}>Delete</button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
};

export default AdminScreenings;
