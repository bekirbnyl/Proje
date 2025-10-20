import React from 'react';
import { useSelector } from 'react-redux';
import { RootState } from '../../store';
import Header from './Header';
import Sidebar from './Sidebar';
import NotificationContainer from '../common/NotificationContainer';

interface LayoutProps {
  children: React.ReactNode;
}

const Layout: React.FC<LayoutProps> = ({ children }) => {
  const { sidebarOpen } = useSelector((state: RootState) => state.ui);
  const { isAuthenticated } = useSelector((state: RootState) => state.auth);

  return (
    <div style={{ minHeight: '100vh', backgroundColor: '#f8fafc' }}>
      <Header />
      <div style={{ display: 'flex', paddingTop: '64px' }}>
        {isAuthenticated && <Sidebar />}
        <main style={{
          flex: 1,
          marginLeft: isAuthenticated ? (sidebarOpen ? '256px' : '64px') : '0',
          transition: 'margin-left 0.3s ease',
          padding: '24px'
        }}>
          <div style={{ maxWidth: '1280px', margin: '0 auto' }}>
            {children}
          </div>
        </main>
      </div>
      <NotificationContainer />
    </div>
  );
};

export default Layout;