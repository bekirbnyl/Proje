import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { useSelector } from 'react-redux';
import { RootState } from '../../store';
import { USER_ROLES } from '../../types';

// Layout components
import Layout from '../layout/Layout';
import AuthLayout from '../layout/AuthLayout';

// Auth components
import LoginPage from '../auth/LoginPage';
import RegisterPage from '../auth/RegisterPage';

// Public components
import HomePage from '../pages/HomePage';
import MovieListPage from '../pages/MovieListPage';

// Protected components
import DashboardPage from '../pages/DashboardPage';
import ProfilePage from '../pages/ProfilePage';
import BookingPage from '../pages/BookingPage';
import SeatSelectionPage from '../pages/SeatSelectionPage';

// Admin components
import AdminDashboard from '../admin/AdminDashboard';
import AdminMovies from '../admin/AdminMovies';
import AdminScreenings from '../admin/AdminScreenings';
import AdminHalls from '../admin/AdminHalls';
import AdminReports from '../admin/AdminReports';
import AdminMemberApprovals from '../admin/AdminMemberApprovals';
import AdminSettings from '../admin/AdminSettings';

// Desk officer components
import DeskDashboard from '../desk/DeskDashboard';
import DeskTicketSales from '../desk/DeskTicketSales';

// Route protection components
import ProtectedRoute from './ProtectedRoute';
import RoleBasedRoute from './RoleBasedRoute';

const AppRouter: React.FC = () => {
  const { isAuthenticated } = useSelector((state: RootState) => state.auth);

  return (
    <Router>
      <Routes>
        {/* Public routes with auth layout */}
        <Route path="/login" element={
          <AuthLayout>
            <LoginPage />
          </AuthLayout>
        } />
        <Route path="/register" element={
          <AuthLayout>
            <RegisterPage />
          </AuthLayout>
        } />

        {/* Public routes with main layout */}
        <Route path="/" element={
          <Layout>
            <HomePage />
          </Layout>
        } />
        <Route path="/movies" element={
          <Layout>
            <MovieListPage />
          </Layout>
        } />

        {/* Protected routes - require authentication */}
        <Route path="/dashboard" element={
          <ProtectedRoute>
            <Layout>
              <DashboardPage />
            </Layout>
          </ProtectedRoute>
        } />
        <Route path="/profile" element={
          <ProtectedRoute>
            <Layout>
              <ProfilePage />
            </Layout>
          </ProtectedRoute>
        } />
        <Route path="/booking/:screeningId" element={
          <ProtectedRoute requireApproved>
            <Layout>
              <BookingPage />
            </Layout>
          </ProtectedRoute>
        } />
        <Route path="/seats/:screeningId" element={
          <ProtectedRoute requireApproved>
            <Layout>
              <SeatSelectionPage />
            </Layout>
          </ProtectedRoute>
        } />

        {/* Admin routes */}
        <Route path="/admin" element={
          <RoleBasedRoute allowedRoles={[USER_ROLES.ADMIN, USER_ROLES.SINEMA_MUDURU]}>
            <Layout>
              <AdminDashboard />
            </Layout>
          </RoleBasedRoute>
        } />
        <Route path="/admin/movies" element={
          <RoleBasedRoute allowedRoles={[USER_ROLES.ADMIN, USER_ROLES.SINEMA_MUDURU]}>
            <Layout>
              <AdminMovies />
            </Layout>
          </RoleBasedRoute>
        } />
        <Route path="/admin/screenings" element={
          <RoleBasedRoute allowedRoles={[USER_ROLES.ADMIN, USER_ROLES.SINEMA_MUDURU]}>
            <Layout>
              <AdminScreenings />
            </Layout>
          </RoleBasedRoute>
        } />
        <Route path="/admin/halls" element={
          <RoleBasedRoute allowedRoles={[USER_ROLES.ADMIN, USER_ROLES.SINEMA_MUDURU, USER_ROLES.GISE_AMIRI]}>
            <Layout>
              <AdminHalls />
            </Layout>
          </RoleBasedRoute>
        } />
        <Route path="/admin/reports" element={
          <RoleBasedRoute allowedRoles={[USER_ROLES.ADMIN, USER_ROLES.YONETIM, USER_ROLES.SINEMA_MUDURU]}>
            <Layout>
              <AdminReports />
            </Layout>
          </RoleBasedRoute>
        } />
        <Route path="/admin/member-approvals" element={
          <RoleBasedRoute allowedRoles={[USER_ROLES.ADMIN]}>
            <Layout>
              <AdminMemberApprovals />
            </Layout>
          </RoleBasedRoute>
        } />
        <Route path="/admin/settings" element={
          <RoleBasedRoute allowedRoles={[USER_ROLES.ADMIN]}>
            <Layout>
              <AdminSettings />
            </Layout>
          </RoleBasedRoute>
        } />

        {/* Desk officer routes */}
        <Route path="/desk" element={
          <RoleBasedRoute allowedRoles={[USER_ROLES.GISE_GOREVLISI, USER_ROLES.GISE_AMIRI, USER_ROLES.SINEMA_MUDURU, USER_ROLES.ADMIN]}>
            <Layout>
              <DeskDashboard />
            </Layout>
          </RoleBasedRoute>
        } />
        <Route path="/desk/ticket-sales" element={
          <RoleBasedRoute allowedRoles={[USER_ROLES.GISE_GOREVLISI, USER_ROLES.GISE_AMIRI, USER_ROLES.SINEMA_MUDURU, USER_ROLES.ADMIN]}>
            <Layout>
              <DeskTicketSales />
            </Layout>
          </RoleBasedRoute>
        } />

        {/* Redirects */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </Router>
  );
};

export default AppRouter;
