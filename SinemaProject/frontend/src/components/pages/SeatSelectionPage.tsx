import React from 'react';
import BookingPage from './BookingPage';

// Reuse BookingPage seat-selection UI (route differs in AppRouter if needed)
const SeatSelectionPage: React.FC = () => {
  return <BookingPage />;
};

export default SeatSelectionPage;
