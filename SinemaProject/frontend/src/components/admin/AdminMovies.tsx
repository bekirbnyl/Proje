import React, { useEffect, useMemo, useState } from 'react';
import { useAppDispatch, useAppSelector } from '../../store/hooks';
import { RootState } from '../../store';
import { 
  fetchMovies, 
  createMovie, 
  updateMovie, 
  deleteMovie 
} from '../../store/slices/movieSlice';

const emptyForm = { title: '', durationMinutes: 90, isActive: true };

const AdminMovies: React.FC = () => {
  const dispatch = useAppDispatch();
  const { movies, isLoading, error } = useAppSelector((state: RootState) => state.movies);
  const [includeDeleted, setIncludeDeleted] = useState(false);
  const [search, setSearch] = useState('');
  const [editingId, setEditingId] = useState<string | null>(null);
  const [form, setForm] = useState({ ...emptyForm });
  const [deleteReason, setDeleteReason] = useState('No longer showing');

  useEffect(() => {
    dispatch(fetchMovies({ active: undefined, includeDeleted }));
  }, [dispatch, includeDeleted]);

  const filteredMovies = useMemo(() => {
    const s = search.trim().toLowerCase();
    if (!s) return movies;
    return movies.filter(m => m.title.toLowerCase().includes(s));
  }, [movies, search]);

  const startCreate = () => {
    setEditingId(null);
    setForm({ ...emptyForm });
  };

  const startEdit = (id: string) => {
    const m = movies.find(x => x.id === id);
    if (!m) return;
    setEditingId(id);
    setForm({ title: m.title, durationMinutes: m.durationMinutes, isActive: m.isActive });
  };

  const submitForm = async (e: React.FormEvent) => {
    e.preventDefault();
    if (editingId) {
      await dispatch(updateMovie({ id: editingId, movieData: form }));
    } else {
      await dispatch(createMovie(form));
    }
    setEditingId(null);
    setForm({ ...emptyForm });
  };

  const onDelete = async (id: string) => {
    if (!deleteReason.trim()) return;
    await dispatch(deleteMovie({ id, reason: deleteReason.trim() }));
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-gray-900">Manage Movies</h1>
        <p className="mt-1 text-sm text-gray-500">Add, edit, and manage the movie catalog</p>
      </div>

      <div className="card">
        <div className="flex flex-col md:flex-row md:items-end md:justify-between gap-4">
          <div className="flex-1">
            <label className="form-label" htmlFor="search">Search</label>
            <input
              id="search"
              className="input-field"
              placeholder="Search by title..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </div>
          <div className="flex items-center gap-4">
            <label className="flex items-center gap-2 text-sm text-gray-700">
              <input
                type="checkbox"
                className="rounded"
                checked={includeDeleted}
                onChange={(e) => setIncludeDeleted(e.target.checked)}
              />
              <span>Include deleted</span>
            </label>
            <button onClick={startCreate} className="btn-primary">
              <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
              </svg>
              New Movie
            </button>
          </div>
        </div>
      </div>

      {/* Form */}
      <div className="card">
        <form onSubmit={submitForm} className="grid grid-cols-1 md:grid-cols-4 gap-4 items-end">
          <div className="md:col-span-2">
            <label className="form-label" htmlFor="title">Title</label>
            <input
              id="title"
              className="input-field"
              value={form.title}
              onChange={(e) => setForm(f => ({ ...f, title: e.target.value }))}
              required
            />
          </div>
          <div>
            <label className="form-label" htmlFor="duration">Duration (min)</label>
            <input
              id="duration"
              type="number"
              min={1}
              className="input-field"
              value={form.durationMinutes}
              onChange={(e) => setForm(f => ({ ...f, durationMinutes: Number(e.target.value) }))}
              required
            />
          </div>
          <div className="flex items-center gap-2">
            <input
              id="active"
              type="checkbox"
              className="rounded"
              checked={form.isActive}
              onChange={(e) => setForm(f => ({ ...f, isActive: e.target.checked }))}
            />
            <label htmlFor="active" className="text-sm text-gray-700">Active</label>
          </div>
          <div className="md:col-span-4 flex justify-end gap-3">
            <button type="submit" className="btn-primary" disabled={isLoading}>
              {editingId ? 'Update Movie' : 'Create Movie'}
            </button>
            {editingId && (
              <button type="button" className="btn-secondary" onClick={() => { setEditingId(null); setForm({ ...emptyForm }); }}>
                Cancel
              </button>
            )}
          </div>
        </form>
        {error && <p className="mt-3 text-sm text-red-600">{error}</p>}
      </div>

      {/* List */}
      <div className="card">
        {isLoading ? (
          <div className="flex justify-center py-8">
            <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600"></div>
          </div>
        ) : filteredMovies.length === 0 ? (
          <div className="text-center text-gray-600">No movies found.</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead>
                <tr className="text-left text-xs font-semibold text-gray-600">
                  <th className="px-4 py-3">Title</th>
                  <th className="px-4 py-3">Duration</th>
                  <th className="px-4 py-3">Active</th>
                  <th className="px-4 py-3">Deleted</th>
                  <th className="px-4 py-3 text-right">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100 text-sm">
                {filteredMovies.map(m => (
                  <tr key={m.id}>
                    <td className="px-4 py-3 font-medium text-gray-900">{m.title}</td>
                    <td className="px-4 py-3">{m.durationMinutes} min</td>
                    <td className="px-4 py-3">
                      <span className={`status-badge ${m.isActive ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-700'} border`}>
                        {m.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                    <td className="px-4 py-3">
                      {m.isDeleted ? (
                        <span className="status-badge bg-red-100 text-red-800 border">Deleted</span>
                      ) : (
                        <span className="text-gray-400">—</span>
                      )}
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex justify-end gap-2">
                        {!m.isDeleted && (
                          <button className="btn-secondary" onClick={() => startEdit(m.id)}>Edit</button>
                        )}
                        {!m.isDeleted && (
                          <button className="btn-danger" onClick={() => onDelete(m.id)}>Delete</button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
        <div className="mt-4">
          <label className="form-label" htmlFor="delete-reason">Delete reason</label>
          <input
            id="delete-reason"
            className="input-field"
            value={deleteReason}
            onChange={(e) => setDeleteReason(e.target.value)}
            placeholder="Provide a reason required by backend for deletion"
          />
        </div>
      </div>
    </div>
  );
};

export default AdminMovies;
