import React, { useEffect, useMemo, useState } from 'react';
import { useAppDispatch, useAppSelector } from '../../store/hooks';
import { RootState } from '../../store';
import { fetchHalls } from '../../store/slices/seatSlice';
import { fetchMovies } from '../../store/slices/movieSlice';
import { fetchScreenings } from '../../store/slices/screeningSlice';
import apiService from '../../services/api';
import { TicketChannel, TicketType, PaymentMethod } from '../../types';

const DeskTicketSales: React.FC = () => {
  const dispatch = useAppDispatch();
  const { halls } = useAppSelector((s: RootState) => s.seats);
  const { movies } = useAppSelector((s: RootState) => s.movies);
  const { screenings, isLoading } = useAppSelector((s: RootState) => s.screenings);

  const [hallId, setHallId] = useState('');
  const [movieId, setMovieId] = useState('');
  const [date, setDate] = useState(new Date().toISOString().split('T')[0]);
  const [screeningId, setScreeningId] = useState('');

  const [paymentMethod, setPaymentMethod] = useState<PaymentMethod>(PaymentMethod.Cash);
  const [totalAmount, setTotalAmount] = useState<number>(0);
  const [ticketLines, setTicketLines] = useState<Array<{ seatId: string; type: TicketType; price: number }>>([
    { seatId: '', type: TicketType.Full, price: 0 }
  ]);
  const [seats, setSeats] = useState<Array<{ id: string; label: string }>>([]);

  useEffect(() => {
    dispatch(fetchHalls());
    dispatch(fetchMovies({ active: true }));
  }, [dispatch]);

  useEffect(() => {
    const d = new Date(date);
    dispatch(fetchScreenings({ date: d, hallId: hallId || undefined, movieId: movieId || undefined }));
  }, [dispatch, date, hallId, movieId]);

  useEffect(() => {
    const sum = ticketLines.reduce((acc, t) => acc + (Number(t.price) || 0), 0);
    setTotalAmount(sum);
  }, [ticketLines]);

  // Fetch seats when screening is selected
  useEffect(() => {
    const fetchSeats = async () => {
      if (!screeningId) {
        setSeats([]);
        return;
      }
      try {
        const screening = screenings.find(s => s.id === screeningId);
        if (screening) {
          const seatLayout = await apiService.getActiveHallLayout(screening.hallId);
          setSeats(seatLayout.seats.map(seat => ({ id: seat.id, label: seat.label })));
        }
      } catch (error) {
        console.error('Error fetching seats:', error);
        setSeats([]);
      }
    };
    fetchSeats();
  }, [screeningId, screenings]);

  // Calculate price when ticket type changes
  const calculatePrice = async (lineIndex: number, ticketType: TicketType) => {
    if (!screeningId) return;
    
    try {
      const basePrice = await apiService.getBasePrice(screeningId);
      let finalPrice = basePrice;
      
      // Apply discounts based on ticket type
      switch (ticketType) {
        case TicketType.Student:
          finalPrice = basePrice * 0.6; // 40% discount
          break;
        case TicketType.Child:
          finalPrice = basePrice * 0.5; // 50% discount for children
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
      // Fallback to manual pricing
      setTicketLines(lines => 
        lines.map((line, i) => 
          i === lineIndex ? { ...line, type: ticketType } : line
        )
      );
    }
  };

  const screeningsFiltered = useMemo(() => screenings, [screenings]);

  const addLine = () => setTicketLines(l => [...l, { seatId: '', type: TicketType.Full, price: 0 }]);
  const removeLine = (idx: number) => setTicketLines(l => l.filter((_, i) => i !== idx));

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!screeningId || ticketLines.length === 0) return;
    
    try {
      const req = {
        screeningId,
        channel: TicketChannel.BoxOffice,
        paymentMethod,
        items: ticketLines
          .filter(t => t.seatId) // Only include lines with selected seats
          .map(t => ({
            seatId: t.seatId,
            ticketType: t.type,
            isVipGuest: false
          }))
      };
      
      if (req.items.length === 0) {
        alert('Please select at least one seat');
        return;
      }
      
      const res = await apiService.sellTickets(req);
      alert(`Successfully sold ${res.items.length} tickets!\nTotal: ₺${res.totalAfter.toFixed(2)}\nPayment Status: ${res.paymentStatus === 1 ? 'Succeeded' : 'Processing'}`);
      
      // Reset form
      setTicketLines([{ seatId: '', type: TicketType.Full, price: 0 }]);
      setTotalAmount(0);
      setScreeningId('');
    } catch (error) {
      console.error('Error selling tickets:', error);
      alert('Error selling tickets. Please try again.');
    }
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-gray-900">Ticket Sales</h1>
        <p className="mt-1 text-sm text-gray-500">Sell tickets at the box office counter</p>
      </div>

      <div className="card">
        <div className="grid grid-cols-1 md:grid-cols-5 gap-4 items-end">
          <div>
            <label className="form-label">Date</label>
            <input type="date" className="input-field" value={date} onChange={(e) => setDate(e.target.value)} />
          </div>
          <div>
            <label className="form-label">Hall</label>
            <select className="input-field" value={hallId} onChange={(e) => setHallId(e.target.value)}>
              <option value="">All</option>
              {halls.map(h => <option key={h.id} value={h.id}>{h.name}</option>)}
            </select>
          </div>
          <div className="md:col-span-2">
            <label className="form-label">Movie</label>
            <select className="input-field" value={movieId} onChange={(e) => setMovieId(e.target.value)}>
              <option value="">All</option>
              {movies.map(m => <option key={m.id} value={m.id}>{m.title}</option>)}
            </select>
          </div>
          <div>
            <label className="form-label">Screening</label>
            <select className="input-field" value={screeningId} onChange={(e) => setScreeningId(e.target.value)}>
              <option value="">Select</option>
              {screeningsFiltered.map(s => (
                <option key={s.id} value={s.id}>{new Date(s.startAt).toLocaleString()} • {s.movieTitle} • {s.hallName}</option>
              ))}
            </select>
          </div>
        </div>
      </div>

      <form onSubmit={submit} className="card space-y-4">
        <h2 className="text-lg font-semibold text-gray-900">Tickets</h2>
        <div className="space-y-3">
          {ticketLines.map((line, idx) => (
            <div key={idx} className="grid grid-cols-1 md:grid-cols-4 gap-3 items-end">
              <div>
                <label className="form-label">Seat</label>
                <select className="input-field" value={line.seatId} onChange={(e) => setTicketLines(ls => ls.map((l,i) => i===idx ? { ...l, seatId: e.target.value } : l))}>
                  <option value="">Select Seat</option>
                  {seats.map(seat => (
                    <option key={seat.id} value={seat.id}>{seat.label}</option>
                  ))}
                </select>
              </div>
              <div>
                <label className="form-label">Type</label>
                <select className="input-field" value={line.type} onChange={(e) => calculatePrice(idx, Number(e.target.value))}>
                  <option value={TicketType.Full}>Full</option>
                  <option value={TicketType.Student}>Student</option>
                  <option value={TicketType.Child}>Child</option>
                </select>
              </div>
              <div>
                <label className="form-label">Price</label>
                <input type="number" min={0} className="input-field" value={line.price} onChange={(e) => setTicketLines(ls => ls.map((l,i) => i===idx ? { ...l, price: Number(e.target.value) } : l))} />
              </div>
              <div className="flex gap-2">
                {ticketLines.length > 1 && (
                  <button type="button" className="btn-secondary" onClick={() => removeLine(idx)}>Remove</button>
                )}
                {idx === ticketLines.length - 1 && (
                  <button type="button" className="btn-primary" onClick={addLine}>Add</button>
                )}
              </div>
            </div>
          ))}
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 items-end">
          <div>
            <label className="form-label">Payment Method</label>
            <select className="input-field" value={paymentMethod} onChange={(e) => setPaymentMethod(Number(e.target.value))}>
              <option value={PaymentMethod.Cash}>Cash</option>
              <option value={PaymentMethod.CreditCard}>Credit Card</option>
              <option value={PaymentMethod.DebitCard}>Debit Card</option>
              <option value={PaymentMethod.MemberCredit}>Member Credit</option>
              <option value={PaymentMethod.BankTransfer}>Bank Transfer</option>
            </select>
          </div>
          <div>
            <label className="form-label">Total Amount</label>
            <input className="input-field" value={totalAmount} readOnly />
          </div>
          <div className="flex justify-end">
            <button className="btn-success" type="submit" disabled={!screeningId || ticketLines.some(t => !t.seatId || t.price <= 0)}>Sell Tickets</button>
          </div>
        </div>
      </form>
    </div>
  );
};

export default DeskTicketSales;
