import React, { useState } from 'react';
import { useAppDispatch, useAppSelector } from '../../store/hooks';
import { RootState } from '../../store';
import { downloadSalesReport, downloadDeletionsReport, downloadMembershipsReport } from '../../store/slices/adminSlice';

const AdminReports: React.FC = () => {
  const dispatch = useAppDispatch();
  const { isLoading, error } = useAppSelector((s: RootState) => s.admin);
  const [dateFrom, setDateFrom] = useState<string>(new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]); // 30 days ago
  const [dateTo, setDateTo] = useState<string>(new Date().toISOString().split('T')[0]); // today

  const salesParams = () => ({
    from: dateFrom || undefined,
    to: dateTo || undefined,
    grain: 'daily',
    by: 'film',
    channel: 'all'
  });

  const deletionsParams = () => ({
    from: dateFrom || undefined,
    to: dateTo || undefined,
    by: 'entity' // Valid options: entity, reason, user
  });

  const membershipsParams = () => ({
    from: dateFrom || undefined,
    to: dateTo || undefined,
    by: 'monthly' // Valid options for memberships
  });

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-gray-900">Reports & Analytics</h1>
        <p className="mt-1 text-sm text-gray-500">View system analytics and export reports</p>
      </div>

      <div className="card">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <div>
            <label className="form-label">From</label>
            <input type="date" className="input-field" value={dateFrom} onChange={(e) => setDateFrom(e.target.value)} />
          </div>
          <div>
            <label className="form-label">To</label>
            <input type="date" className="input-field" value={dateTo} onChange={(e) => setDateTo(e.target.value)} />
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="card-simple">
          <h3 className="text-lg font-semibold text-gray-900 mb-3">Sales Report</h3>
          <p className="text-sm text-gray-600 mb-4">Export ticket sales in Excel format.</p>
          <button className="btn-primary" disabled={isLoading} onClick={() => dispatch(downloadSalesReport(salesParams()))}>Download</button>
        </div>
        <div className="card-simple">
          <h3 className="text-lg font-semibold text-gray-900 mb-3">Deletions Report</h3>
          <p className="text-sm text-gray-600 mb-4">Export deletions and voids in Excel format.</p>
          <button className="btn-primary" disabled={isLoading} onClick={() => dispatch(downloadDeletionsReport(deletionsParams()))}>Download</button>
        </div>
        <div className="card-simple">
          <h3 className="text-lg font-semibold text-gray-900 mb-3">Memberships Report</h3>
          <p className="text-sm text-gray-600 mb-4">Export member statistics in Excel format.</p>
          <button className="btn-primary" disabled={isLoading} onClick={() => dispatch(downloadMembershipsReport(membershipsParams()))}>Download</button>
        </div>
      </div>

      {error && <p className="text-sm text-red-600">{error}</p>}
    </div>
  );
};

export default AdminReports;
