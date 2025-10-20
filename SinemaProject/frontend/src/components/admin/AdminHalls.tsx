import React, { useEffect, useMemo, useState } from 'react';
import { useAppDispatch, useAppSelector } from '../../store/hooks';
import { RootState } from '../../store';
import { fetchHalls, fetchHallSeatLayouts, activateSeatLayout, addSeatLayoutVersion } from '../../store/slices/seatSlice';

const AdminHalls: React.FC = () => {
  const dispatch = useAppDispatch();
  const { halls, seatLayouts, isLoading, error } = useAppSelector((s: RootState) => s.seats);
  const [selectedHall, setSelectedHall] = useState<string>('');
  const [rowsInput, setRowsInput] = useState<number>(5);
  const [colsInput, setColsInput] = useState<number>(8);

  useEffect(() => {
    dispatch(fetchHalls());
  }, [dispatch]);

  useEffect(() => {
    if (selectedHall) {
      dispatch(fetchHallSeatLayouts(selectedHall));
    }
  }, [dispatch, selectedHall]);

  const layouts = useMemo(() => selectedHall ? (seatLayouts[selectedHall] || []) : [], [seatLayouts, selectedHall]);

  const onActivate = async (layoutId: string) => {
    await dispatch(activateSeatLayout({ hallId: selectedHall, layoutId }));
  };

  const onAddLayout = async () => {
    if (!selectedHall) return;
    // Simple generator: rows x cols grid of active seats with labels RowCol
    const seats = [] as any[];
    for (let r = 1; r <= rowsInput; r++) {
      for (let c = 1; c <= colsInput; c++) {
        seats.push({ row: String.fromCharCode(64 + r), col: String(c), label: `${String.fromCharCode(64 + r)}${c}`, isActive: true });
      }
    }
    await dispatch(addSeatLayoutVersion({ hallId: selectedHall, seats }));
    dispatch(fetchHallSeatLayouts(selectedHall));
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-gray-900">Manage Halls</h1>
        <p className="mt-1 text-sm text-gray-500">Configure cinema halls and seat layouts</p>
      </div>

      {/* Hall selector */}
      <div className="card">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 items-end">
          <div className="md:col-span-2">
            <label className="form-label">Select Hall</label>
            <select className="input-field" value={selectedHall} onChange={(e) => setSelectedHall(e.target.value)}>
              <option value="">Choose a hall</option>
              {halls.map(h => (
                <option key={h.id} value={h.id}>{h.name}</option>
              ))}
            </select>
          </div>
        </div>
      </div>

      {/* Layouts list */}
      <div className="card">
        {isLoading && !selectedHall ? (
          <div className="flex justify-center py-8"><div className="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600"></div></div>
        ) : !selectedHall ? (
          <div className="text-gray-600">Select a hall to view layouts.</div>
        ) : layouts.length === 0 ? (
          <div className="text-gray-600">No layouts available for this hall.</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead>
                <tr className="text-left text-xs font-semibold text-gray-600">
                  <th className="px-4 py-3">Layout ID</th>
                  <th className="px-4 py-3">Version</th>
                  <th className="px-4 py-3">Active</th>
                  <th className="px-4 py-3">Created</th>
                  <th className="px-4 py-3 text-right">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100 text-sm">
                {layouts.map(l => (
                  <tr key={l.id}>
                    <td className="px-4 py-3 font-medium text-gray-900">{l.id}</td>
                    <td className="px-4 py-3">{l.version}</td>
                    <td className="px-4 py-3">{l.isActive ? <span className="status-badge bg-green-100 text-green-800 border">Active</span> : <span className="status-badge bg-gray-100 text-gray-700 border">Inactive</span>}</td>
                    <td className="px-4 py-3">{new Date(l.createdAt).toLocaleString()}</td>
                    <td className="px-4 py-3">
                      <div className="flex justify-end gap-2">
                        {!l.isActive && <button className="btn-primary" onClick={() => onActivate(l.id)}>Activate</button>}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
        {error && <p className="mt-3 text-sm text-red-600">{error}</p>}
      </div>

      {/* Add layout */}
      <div className="card">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Add New Layout Version</h3>
        <div className="grid grid-cols-1 md:grid-cols-5 gap-4 items-end">
          <div>
            <label className="form-label">Rows</label>
            <input type="number" min={1} className="input-field" value={rowsInput} onChange={(e) => setRowsInput(Number(e.target.value))} />
          </div>
          <div>
            <label className="form-label">Columns</label>
            <input type="number" min={1} className="input-field" value={colsInput} onChange={(e) => setColsInput(Number(e.target.value))} />
          </div>
          <div className="md:col-span-2 text-sm text-gray-600">Generates a simple grid of active seats as a starting point.</div>
          <div className="flex justify-end">
            <button className="btn-primary" onClick={onAddLayout} disabled={!selectedHall}>Add Version</button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default AdminHalls;
