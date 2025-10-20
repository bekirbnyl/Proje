import React, { useEffect, useState } from 'react';
import { useAppDispatch, useAppSelector } from '../../store/hooks';
import { RootState } from '../../store';
import { fetchScreenings } from '../../store/slices/screeningSlice';
import { Link } from 'react-router-dom';

const DeskDashboard: React.FC = () => {
  const dispatch = useAppDispatch();
  const { screenings, isLoading } = useAppSelector((s: RootState) => s.screenings);
  const [selectedDate, setSelectedDate] = useState<string>(new Date().toISOString().split('T')[0]);

  useEffect(() => {
    dispatch(fetchScreenings({ date: new Date(selectedDate) }));
  }, [dispatch, selectedDate]);

  const formatTime = (dateString: string) => new Date(dateString).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit', hour12: false });

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-gray-900">Box Office Dashboard</h1>
        <p className="mt-1 text-sm text-gray-500">Manage ticket sales and box office operations</p>
      </div>

      <div className="card">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 items-end">
          <div>
            <label className="form-label">Date</label>
            <input type="date" className="input-field" value={selectedDate} onChange={(e) => setSelectedDate(e.target.value)} />
          </div>
          <div className="md:col-span-2 flex justify-end">
            <Link to="/desk/ticket-sales" className="btn-primary">Go to Ticket Sales</Link>
          </div>
        </div>
      </div>

      <div className="card">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Today's Screenings</h2>
        {isLoading ? (
          <div className="flex justify-center py-8"><div className="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600"></div></div>
        ) : screenings.length === 0 ? (
          <div className="text-gray-600">No screenings found.</div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {screenings.map(s => (
              <div key={s.id} className="card-simple">
                <div className="flex items-center justify-between mb-2">
                  <h3 className="font-semibold text-gray-900">{s.movieTitle}</h3>
                  <span className="text-xs text-gray-500">{formatTime(s.startAt)}</span>
                </div>
                <p className="text-sm text-gray-600">{s.hallName} • {s.cinemaName}</p>
                <div className="mt-3 flex justify-end">
                  <Link to="/desk/ticket-sales" className="btn-secondary text-sm">Sell Tickets</Link>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default DeskDashboard;
