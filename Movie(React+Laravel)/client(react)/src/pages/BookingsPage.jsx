import { useState, useEffect } from 'react';
import { Container, Table, Button, Alert, Badge, Card } from 'react-bootstrap';
import BookingService from '../services/bookingService';

const BookingsPage = () => {
  const [bookings, setBookings] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchBookings = async () => {
      setLoading(true);
      try {
        const data = await BookingService.getAllBookings();
        setBookings(data);
      } catch (err) {
        setError('Failed to fetch bookings');
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    fetchBookings();
  }, []);

  const handleCancelBooking = async (id) => {
    if (window.confirm('Biztosan törölni szeretnéd ezt a foglalást?')) {
      try {
        await BookingService.deleteBooking(id);
        setBookings(bookings.filter(booking => booking.id !== id));
      } catch (err) {
        setError('Failed to cancel booking');
        console.error(err);
      }
    }
  };

  const formatDateTime = (screening) => {
    if (!screening) return 'N/A';
    
    try {
      if (screening.date && screening.start_time) {
        const date = new Date(screening.date);
        let timeString = screening.start_time;
       
        if (timeString.includes('T')) {
          const timeParts = timeString.split('T')[1].split(':');
          timeString = `${timeParts[0]}:${timeParts[1]}`;
        }
        
        if (isNaN(date.getTime())) {
          console.error('Invalid date:', screening.date);
          return 'Érvénytelen dátum';
        }
        
        const dateOptions = { year: 'numeric', month: 'long', day: 'numeric' };
        const formattedDate = date.toLocaleDateString('hu-HU', dateOptions);
        
        return `${formattedDate}, ${timeString}`;
      }
      
      if (screening.start_time) {
        const dateTime = new Date(screening.start_time);
        
        if (isNaN(dateTime.getTime())) {
          console.error('Invalid datetime:', screening.start_time);
          return 'Érvénytelen dátum';
        }
        
        const options = { 
          year: 'numeric', 
          month: 'long', 
          day: 'numeric', 
          hour: '2-digit', 
          minute: '2-digit' 
        };
        return dateTime.toLocaleDateString('hu-HU', options);
      }
      
      return 'N/A';
    } catch (error) {
      console.error('Date formatting error:', error, { screening });
      return 'Dátum hiba';
    }
  };

  const formatSeats = (seats) => {
    if (!seats || seats.length === 0) return 'N/A';
    
    return seats.map(seat => {
      const seatNumber = seat.seat || seat.number;
      const rowLetter = String.fromCharCode(64 + seat.row); 
      return `${rowLetter}${seatNumber}`;
    }).join(', ');
  };

  const formatTicketTypes = (ticketTypes) => {
    if (!ticketTypes || !Array.isArray(ticketTypes) || ticketTypes.length === 0) {
      return 'N/A';
    }
    
    return ticketTypes.map((ticket, idx) => {
      const quantity = ticket.quantity || ticket.count || 1;
      const type = ticket.type || 'Ismeretlen';
      
      return (
        <div key={idx} className="mb-1">
          {type}: {quantity} db
        </div>
      );
    });
  };

  const formatPrice = (price) => {
    if (price === null || price === undefined) {
      return 'N/A';
    }
    
    const numPrice = typeof price === 'string' ? parseFloat(price) : price;
    
    if (isNaN(numPrice)) {
      return 'N/A';
    }
    
    return `${numPrice.toLocaleString('hu-HU')} Ft`;
  };

  if (loading) {
    return (
      <Container className="d-flex justify-content-center align-items-center" style={{ minHeight: '60vh' }}>
        <div className="text-center">
          <div className="spinner-border text-primary mb-3" role="status">
            <span className="visually-hidden">Loading...</span>
          </div>
          <p className="text-muted">Foglalások betöltése...</p>
        </div>
      </Container>
    );
  }

  return (
    <Container>
      <h1 className="mb-4">Foglalásaim</h1>
      
      {error && <Alert variant="danger">{error}</Alert>}
      
      {bookings.length === 0 ? (
        <Alert variant="info">
          <i className="bi bi-info-circle me-2"></i>
          Nincsenek foglalásaid.
        </Alert>
      ) : (
        <>
          {/* Mobile card view for small screens */}
          <div className="d-md-none">
            {bookings.map(booking => (
              <Card key={booking.id} className="mb-3 shadow-sm">
                <Card.Body>
                  <div className="d-flex justify-content-between align-items-start mb-2">
                    <h5 className="mb-0">{booking.screening?.movie?.title || 'N/A'}</h5>
                    <Badge bg={booking.status === 'confirmed' ? 'success' : 'warning'}>
                      {booking.status === 'confirmed' ? 'Megerősítve' : booking.status}
                    </Badge>
                  </div>
                  
                  <div className="mb-2">
                    <small className="text-muted">Időpont:</small><br />
                    <strong>{formatDateTime(booking.screening)}</strong>
                  </div>
                  
                  <div className="mb-2">
                    <small className="text-muted">Terem:</small><br />
                    {booking.screening?.room?.name || 'N/A'}
                  </div>
                  
                  <div className="mb-2">
                    <small className="text-muted">Ülőhelyek:</small><br />
                    <span className="badge bg-secondary">
                      {formatSeats(booking.seats)}
                    </span>
                  </div>
                  
                  <div className="mb-2">
                    <small className="text-muted">Jegyek:</small><br />
                    {formatTicketTypes(booking.ticket_types)}
                  </div>
                  
                  <div className="d-flex justify-content-between align-items-center">
                    <div>
                      <small className="text-muted">Összeg:</small><br />
                      <strong className="text-success fs-5">{formatPrice(booking.total_price)}</strong>
                    </div>
                    <Button 
                      variant="outline-danger" 
                      size="sm"
                      onClick={() => handleCancelBooking(booking.id)}
                    >
                      <i className="bi bi-trash me-1"></i>
                      Lemondás
                    </Button>
                  </div>
                </Card.Body>
              </Card>
            ))}
          </div>
          
          {/* Desktop table view for larger screens */}
          <div className="d-none d-md-block">
            <div className="table-responsive">
              <Table striped bordered hover>
                <thead className="table-dark">
                  <tr>
                    <th>Film</th>
                    <th>Időpont</th>
                    <th>Terem</th>
                    <th>Ülőhelyek</th>
                    <th>Jegyek</th>
                    <th>Összeg</th>
                    <th>Státusz</th>
                    <th>Műveletek</th>
                  </tr>
                </thead>
                <tbody>
                  {bookings.map(booking => (
                    <tr key={booking.id}>
                      <td>
                        <strong>{booking.screening?.movie?.title || 'N/A'}</strong>
                      </td>
                      <td>{formatDateTime(booking.screening)}</td>
                      <td>{booking.screening?.room?.name || 'N/A'}</td>
                      <td>
                        <span className="badge bg-secondary">
                          {formatSeats(booking.seats)}
                        </span>
                      </td>
                      <td>{formatTicketTypes(booking.ticket_types)}</td>
                      <td>
                        <strong className="text-success">{formatPrice(booking.total_price)}</strong>
                      </td>
                      <td>
                        <Badge bg={booking.status === 'confirmed' ? 'success' : 'warning'}>
                          {booking.status === 'confirmed' ? 'Megerősítve' : booking.status}
                        </Badge>
                      </td>
                      <td>
                        <Button 
                          variant="outline-danger" 
                          size="sm"
                          onClick={() => handleCancelBooking(booking.id)}
                        >
                          <i className="bi bi-trash me-1"></i>
                          Lemondás
                        </Button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </Table>
            </div>
          </div>
        </>
      )}
    </Container>
  );
};

export default BookingsPage; 