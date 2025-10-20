import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { MemberApproval } from '../../types';
import apiService from '../../services/api';

interface AdminState {
  pendingApprovals: MemberApproval[];
  isLoading: boolean;
  error: string | null;
}

const initialState: AdminState = {
  pendingApprovals: [],
  isLoading: false,
  error: null,
};

// Async thunks
export const fetchPendingMemberApprovals = createAsyncThunk(
  'admin/fetchPendingMemberApprovals',
  async (_, { rejectWithValue }) => {
    try {
      const approvals = await apiService.getPendingMemberApprovals();
      return approvals;
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Failed to fetch pending approvals');
    }
  }
);

export const approveMember = createAsyncThunk(
  'admin/approveMember',
  async ({ memberId, reason }: { memberId: string; reason: string }, { rejectWithValue }) => {
    try {
      await apiService.approveMember(memberId, reason);
      return memberId;
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Failed to approve member');
    }
  }
);

export const rejectMember = createAsyncThunk(
  'admin/rejectMember',
  async ({ memberId, reason }: { memberId: string; reason: string }, { rejectWithValue }) => {
    try {
      await apiService.rejectMember(memberId, reason);
      return memberId;
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Failed to reject member');
    }
  }
);

export const grantVipStatus = createAsyncThunk(
  'admin/grantVipStatus',
  async (memberId: string, { rejectWithValue }) => {
    try {
      await apiService.grantVipStatus(memberId);
      return memberId;
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Failed to grant VIP status');
    }
  }
);

export const revokeVipStatus = createAsyncThunk(
  'admin/revokeVipStatus',
  async (memberId: string, { rejectWithValue }) => {
    try {
      await apiService.revokeVipStatus(memberId);
      return memberId;
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Failed to revoke VIP status');
    }
  }
);

export const approveVipApplication = createAsyncThunk(
  'admin/approveVipApplication',
  async ({ approvalId, reason }: { approvalId: string; reason: string }, { rejectWithValue }) => {
    try {
      await apiService.approveVipApplication(approvalId, reason);
      return approvalId;
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Failed to approve VIP application');
    }
  }
);

export const downloadSalesReport = createAsyncThunk(
  'admin/downloadSalesReport',
  async (params: any, { rejectWithValue }) => {
    try {
      const blob = await apiService.downloadSalesReport(params);
      const filename = `sales-report-${new Date().toISOString().split('T')[0]}.xlsx`;
      apiService.downloadBlob(blob, filename);
      return filename;
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Failed to download sales report');
    }
  }
);

export const downloadDeletionsReport = createAsyncThunk(
  'admin/downloadDeletionsReport',
  async (params: any, { rejectWithValue }) => {
    try {
      const blob = await apiService.downloadDeletionsReport(params);
      const filename = `deletions-report-${new Date().toISOString().split('T')[0]}.xlsx`;
      apiService.downloadBlob(blob, filename);
      return filename;
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Failed to download deletions report');
    }
  }
);

export const downloadMembershipsReport = createAsyncThunk(
  'admin/downloadMembershipsReport',
  async (params: any, { rejectWithValue }) => {
    try {
      const blob = await apiService.downloadMembershipsReport(params);
      const filename = `memberships-report-${new Date().toISOString().split('T')[0]}.xlsx`;
      apiService.downloadBlob(blob, filename);
      return filename;
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Failed to download memberships report');
    }
  }
);

const adminSlice = createSlice({
  name: 'admin',
  initialState,
  reducers: {
    clearError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    // Fetch pending member approvals
    builder
      .addCase(fetchPendingMemberApprovals.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchPendingMemberApprovals.fulfilled, (state, action) => {
        state.isLoading = false;
        state.pendingApprovals = action.payload;
        state.error = null;
      })
      .addCase(fetchPendingMemberApprovals.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Approve member
    builder
      .addCase(approveMember.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(approveMember.fulfilled, (state, action) => {
        state.isLoading = false;
        const memberId = action.payload;
        state.pendingApprovals = state.pendingApprovals.filter(
          approval => approval.memberId !== memberId
        );
        state.error = null;
      })
      .addCase(approveMember.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Reject member
    builder
      .addCase(rejectMember.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(rejectMember.fulfilled, (state, action) => {
        state.isLoading = false;
        const memberId = action.payload;
        state.pendingApprovals = state.pendingApprovals.filter(
          approval => approval.memberId !== memberId
        );
        state.error = null;
      })
      .addCase(rejectMember.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Grant VIP status
    builder
      .addCase(grantVipStatus.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(grantVipStatus.fulfilled, (state) => {
        state.isLoading = false;
        state.error = null;
      })
      .addCase(grantVipStatus.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Revoke VIP status
    builder
      .addCase(revokeVipStatus.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(revokeVipStatus.fulfilled, (state) => {
        state.isLoading = false;
        state.error = null;
      })
      .addCase(revokeVipStatus.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    // Download reports
    builder
      .addCase(downloadSalesReport.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(downloadSalesReport.fulfilled, (state) => {
        state.isLoading = false;
        state.error = null;
      })
      .addCase(downloadSalesReport.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    builder
      .addCase(downloadDeletionsReport.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(downloadDeletionsReport.fulfilled, (state) => {
        state.isLoading = false;
        state.error = null;
      })
      .addCase(downloadDeletionsReport.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });

    builder
      .addCase(downloadMembershipsReport.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(downloadMembershipsReport.fulfilled, (state) => {
        state.isLoading = false;
        state.error = null;
      })
      .addCase(downloadMembershipsReport.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });
  },
});

export const { clearError } = adminSlice.actions;
export default adminSlice.reducer;
