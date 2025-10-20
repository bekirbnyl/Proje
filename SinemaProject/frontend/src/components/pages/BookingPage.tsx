import React, { useEffect, useState, useMemo } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../../store/hooks';
import { RootState } from '../../store';
import { fetchScreening } from '../../store/slices/screeningSlice';
import { addNotification } from '../../store/slices/uiSlice';
import apiService from '../../services/api';
import { TicketType } from '../../types';

interface TicketLine {
  seatId: string;
  type: TicketType;
  price: number;
}

const BookingPage: React.FC = () => {
  const { screeningId } = useParams();
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { currentScreening } = useAppSelector((s: RootState) => s.screenings);
  const { user } = useAppSelector((s: RootState) => s.auth);

  const [seats, setSeats] = useState<Array<{ id: string; label: string; row: number; col: number }>>([]);
  const [ticketLines, setTicketLines] = useState<TicketLine[]>([{ seatId: '', type: TicketType.Full, price: 0 }]);
  const [isLoading, setIsLoading] = useState(false);
  const [paymentMethod, setPaymentMethod] = useState<number>(1); // 1 = CreditCard

  useEffect(() => {
    if (!screeningId) return;
    dispatch(fetchScreening(screeningId));
  }, [dispatch, screeningId]);

  useEffect(() => {
    const fetchSeats = async () => {
      if (!currentScreening?.hallId) {
        setSeats([]);
        return;
      }
      try {
        const seatLayout = await apiService.getActiveHallLayout(currentScreening.hallId);
        setSeats(seatLayout.seats.map(seat => ({
          id: seat.id,
          label: seat.label,
          row: seat.row,
          col: seat.col
        })).sort((a, b) => a.row - b.row || a.col - b.col));
      } catch (error) {
        console.error('Error fetching seats:', error);
        setSeats([]);
      }
    };
    fetchSeats();
  }, [currentScreening]);

  const calculatePrice = async (lineIndex: number, ticketType: TicketType) => {
    if (!screeningId) return;

    try {
      const basePrice = await apiService.getBasePrice(screeningId);
      let finalPrice = basePrice;

      switch (ticketType) {
        case TicketType.Student:
          finalPrice = basePrice * 0.6;
          break;
        case TicketType.Child:
          finalPrice = basePrice * 0.5;
          break;
        default:
          finalPrice = basePrice;
      }

      setTicketLines(lines =>
        lines.map((line, i) =>
          i === lineIndex ? { ...line, type: ticketType, price: finalPrice } : line
        )
      );
    } catch (error) {
      console.error('Error calculating price:', error);
    }
  };

  const addLine = () => setTicketLines(l => [...l, { seatId: '', type: TicketType.Full, price: 0 }]);
  const removeLine = (idx: number) => setTicketLines(l => l.filter((_, i) => i !== idx));

  const updateSeat = (idx: number, seatId: string) => {
    setTicketLines(lines => lines.map((line, i) => i === idx ? { ...line, seatId } : line));
  };

  const totalAmount = useMemo(() => {
    return ticketLines.reduce((sum, line) => sum + (Number(line.price) || 0), 0);
  }, [ticketLines]);

  const getAvailableSeats = (currentLineIndex: number) => {
    const selectedSeatIds = ticketLines
      .map((line, idx) => idx !== currentLineIndex ? line.seatId : null)
      .filter(Boolean);
    return seats.filter(seat => !selectedSeatIds.includes(seat.id));
  };

  const handlePurchase = async () => {
    if (!screeningId || !user?.memberId) {
      dispatch(addNotification({
        type: 'error',
        title: 'Error',
        message: 'Missing required information',
      }));
      return;
    }

    const validLines = ticketLines.filter(line => line.seatId);
    if (validLines.length === 0) {
      dispatch(addNotification({
        type: 'error',
        title: 'No Seats Selected',
        message: 'Please select at least one seat',
      }));
      return;
    }

    setIsLoading(true);

    try {
      // Prepare ticket items
      const items = validLines.map(line => ({
        seatId: line.seatId,
        ticketType: line.type,
      }));

      // Call sell tickets API directly
      const response = await apiService.sellTickets({
        screeningId,
        memberId: user.memberId,
        items,
        paymentMethod,
        channel: 0, // Online
      });

      dispatch(addNotification({
        type: 'success',
        title: 'Tickets Purchased!',
        message: `${response.tickets.length} ticket(s) purchased successfully! Total: ₺${totalAmount.toFixed(2)}`,
      }));

      // Redirect to dashboard immediately
      setTimeout(() => {
        navigate('/dashboard');
      }, 1500);

    } catch (error: any) {
      dispatch(addNotification({
        type: 'error',
        title: 'Purchase Failed',
        message: error.message || 'Failed to purchase tickets. Please try again.',
      }));
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-gray-900">Book Tickets</h1>
        <p className="mt-1 text-sm text-gray-500">Select your seats and ticket types</p>
      </div>

      {/* Screening Info */}
      <div className="card">
        {!currentScreening ? (
          <div className="flex justify-center py-8">
            <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600"></div>
          </div>
        ) : (
          <div className="flex items-center justify-between">
            <div>
              <h3 className="text-lg font-semibold text-gray-900">{currentScreening.movie?.title}</h3>
              <p className="text-sm text-gray-600">
                Hall: {currentScreening.hall?.name} • Starts: {new Date(currentScreening.startAt).toLocaleString()}
              </p>
            </div>
          </div>
        )}
      </div>

      {/* Ticket Lines */}
      <div className="card">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-semibold text-gray-900">Select Seats</h2>
          <button
            onClick={addLine}
            className="btn-primary text-sm"
            disabled={isLoading}
          >
            + Add Seat
          </button>
        </div>

        <div className="space-y-3">
          {ticketLines.map((line, idx) => {
            const availableSeats = getAvailableSeats(idx);
            return (
              <div key={idx} className="grid grid-cols-1 md:grid-cols-12 gap-3 items-center p-3 bg-gray-50 rounded-lg">
                <div className="md:col-span-1 text-sm font-medium text-gray-700">
                  #{idx + 1}
                </div>

                <div className="md:col-span-5">
                  <select
                    value={line.seatId}
                    onChange={(e) => updateSeat(idx, e.target.value)}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    disabled={isLoading}
                  >
                    <option value="">Select a seat</option>
                    {availableSeats.map(seat => (
                      <option key={seat.id} value={seat.id}>
                        {seat.label}
                      </option>
                    ))}
                  </select>
                </div>

                <div className="md:col-span-3">
                  <select
                    value={line.type}
                    onChange={(e) => calculatePrice(idx, Number(e.target.value))}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    disabled={isLoading}
                  >
                    <option value={TicketType.Full}>Full Price</option>
                    <option value={TicketType.Student}>Student (40% off)</option>
                    <option value={TicketType.Child}>Child (50% off)</option>
                  </select>
                </div>

                <div className="md:col-span-2 text-right">
                  <span className="font-semibold text-gray-900">
                    {line.price > 0 ? `₺${line.price.toFixed(2)}` : '-'}
                  </span>
                </div>

                <div className="md:col-span-1 flex justify-end">
                  {ticketLines.length > 1 && (
                    <button
                      onClick={() => removeLine(idx)}
                      className="text-red-600 hover:text-red-800"
                      disabled={isLoading}
                    >
                      <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                      </svg>
                    </button>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      </div>

      {/* Payment Method */}
      <div className="card">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Payment Method</h2>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          {[
            { value: 1, label: 'Credit Card' },
            { value: 2, label: 'Debit Card' },
            { value: 3, label: 'Member Credit' },
            { value: 4, label: 'Bank Transfer' },
          ].map((method) => (
            <label
              key={method.value}
              className={`flex items-center justify-center p-3 border-2 rounded-lg cursor-pointer transition ${
                paymentMethod === method.value
                  ? 'border-blue-500 bg-blue-50'
                  : 'border-gray-200 hover:border-gray-300'
              }`}
            >
              <input
                type="radio"
                name="paymentMethod"
                value={method.value}
                checked={paymentMethod === method.value}
                onChange={(e) => setPaymentMethod(Number(e.target.value))}
                className="mr-2"
              />
              <span className="text-sm font-medium text-gray-900">{method.label}</span>
            </label>
          ))}
        </div>
      </div>

      {/* Summary and Action */}
      <div className="card bg-blue-50">
        <div className="flex flex-col sm:flex-row items-center justify-between gap-4">
          <div>
            <p className="text-sm text-gray-600">Total Amount</p>
            <p className="text-3xl font-bold text-gray-900">₺{totalAmount.toFixed(2)}</p>
            <p className="text-sm text-gray-600 mt-1">
              {ticketLines.filter(l => l.seatId).length} seat(s) selected
            </p>
          </div>
          <div className="flex gap-3">
            <button
              onClick={() => navigate('/')}
              className="btn-secondary"
              disabled={isLoading}
            >
              Cancel
            </button>
            <button
              onClick={handlePurchase}
              className="btn-success px-8 font-semibold"
              disabled={isLoading || ticketLines.filter(l => l.seatId).length === 0}
            >
              {isLoading ? 'Purchasing...' : '🎫 Purchase Tickets'}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default BookingPage;
