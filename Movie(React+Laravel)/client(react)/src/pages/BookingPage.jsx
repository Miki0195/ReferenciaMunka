import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Container, Card, Row, Col, Button, Form, Alert, Table, Badge } from 'react-bootstrap';
import ScreeningService from '../services/screeningService';
import BookingService from '../services/bookingService';
import TicketTypeService from '../services/ticketTypeService';
import { useAuth } from '../hooks/useAuth';
import MovieService from '../services/movieService';
import RoomService from '../services/roomService';

const BookingPage = () => {
  const [screening, setScreening] = useState(null);
  const [movie, setMovie] = useState(null);
  const [room, setRoom] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [selectedSeats, setSelectedSeats] = useState([]);
  const [occupiedSeats, setOccupiedSeats] = useState([]);
  const [ticketTypes, setTicketTypes] = useState([]);
  const [selectedTickets, setSelectedTickets] = useState([
    { type: 'normal', quantity: 1 }
  ]);
  const [step, setStep] = useState(1); 
  const [basePrice, setBasePrice] = useState(1500); 
  
  const { id } = useParams();
  const navigate = useNavigate();
  const { currentUser } = useAuth();

  const isMovieDataComplete = (movieData) => {
    return movieData && 
           movieData.id && 
           movieData.title && 
           typeof movieData.duration !== 'undefined' && 
           typeof movieData.genre !== 'undefined';
  };

  const isScreeningInPast = (screening) => {
    if (!screening || !screening.date || !screening.start_time) {
      return true;
    }
    
    try {
      // Ensure start_time is in HH:MM format
      let timeString = screening.start_time;
      if (timeString.includes('T')) {
        // If it's a full datetime, extract just the time part
        timeString = timeString.split('T')[1].split(':')[0] + ':' + timeString.split('T')[1].split(':')[1];
      }
      
      const screeningDateTime = new Date(`${screening.date}T${timeString}:00`);
      const now = new Date();
      
      return screeningDateTime < now;
    } catch (error) {
      console.error('Error checking screening time:', error, { date: screening.date, start_time: screening.start_time });
      return true; 
    }
  };

  useEffect(() => {
    const fetchData = async () => {
      setLoading(true);
      try {
        const screeningData = await ScreeningService.getScreening(id);
        console.log('Screening data for booking:', screeningData);
        
        if (!screeningData) {
          throw new Error('Vetítés adatok nem elérhetőek');
        }
        
        setScreening(screeningData);
        
        let movieData = null;
        
        
        if (screeningData.movie && isMovieDataComplete(screeningData.movie)) {
          console.log('Using movie data from screening:', screeningData.movie);
          movieData = screeningData.movie;
        } 
        
        else if (screeningData.movie_id) {
          try {
            console.log(`Fetching movie data directly for ID: ${screeningData.movie_id}`);
            const fetchedMovieData = await MovieService.getMovieWithScreenings(screeningData.movie_id);
            
            if (fetchedMovieData && isMovieDataComplete(fetchedMovieData)) {
              console.log('Successfully fetched movie data:', fetchedMovieData);
              movieData = fetchedMovieData;
            } else {
              console.warn('Fetched movie data is incomplete:', fetchedMovieData);
        
              movieData = {
                ...fetchedMovieData
              };
            }
          } catch (movieError) {
            console.error('Failed to fetch movie data:', movieError);
        
          }
        } else {
          console.warn('No movie ID available in screening data');
        }
        
        
        setMovie(movieData);
        
        
        if (screeningData.room) {
        
          const roomData = {
            id: screeningData.room.id || screeningData.room_id || 0,
            name: screeningData.room.name || `Terem ${screeningData.room.id || screeningData.room_id || 'ismeretlen'}`,
            rows: parseInt(screeningData.room.rows || screeningData.room.rows) || 10,
            seats_per_row: parseInt(screeningData.room.seats_per_row || screeningData.room.seatsPerRow) || 10
          };
          setRoom(roomData);
          console.log('Room data for booking:', roomData);
        } else {
          console.error('Room data missing in screening');
          
        
          if (screeningData.room_id) {
            try {
              const roomData = await RoomService.getRoom(screeningData.room_id);
              if (roomData) {
                setRoom(roomData);
                console.log('Fetched room data directly:', roomData);
              } else {
                throw new Error('Terem információ nem elérhető');
              }
            } catch (roomError) {
              console.error('Failed to fetch room directly:', roomError);
              throw new Error('Terem információ nem elérhető');
            }
          } else {
            throw new Error('Terem információ nem elérhető');
          }
        }
        
        
        if (screeningData.bookings && Array.isArray(screeningData.bookings)) {
          setOccupiedSeats(screeningData.bookings);
          console.log('Occupied seats directly from screening resource:', screeningData.bookings);
        } else {
          console.warn('No booking information available');
          setOccupiedSeats([]);
        }
        
        try {
          const ticketTypesData = await TicketTypeService.getAllTicketTypes();
          if (Array.isArray(ticketTypesData) && ticketTypesData.length > 0) {
            setTicketTypes(ticketTypesData);
          } else {       
            setTicketTypes([
              { name: 'normal', display_name: 'Normál jegy', price_multiplier: 1 },
              { name: 'student', display_name: 'Diák jegy', price_multiplier: 0.75 },
              { name: 'senior', display_name: 'Nyugdíjas jegy', price_multiplier: 0.8 }
            ]);
          }
        } catch (ticketError) {
          console.error('Failed to fetch ticket types:', ticketError);
          setTicketTypes([
            { name: 'normal', display_name: 'Normál jegy', price_multiplier: 1 },
            { name: 'student', display_name: 'Diák jegy', price_multiplier: 0.75 },
            { name: 'senior', display_name: 'Nyugdíjas jegy', price_multiplier: 0.8 }
          ]);
        }
        
        if (screeningData.base_price) {
          setBasePrice(parseInt(screeningData.base_price) || 1500);
        }
      } catch (err) {
        setError('Failed to fetch screening data: ' + (err.message || String(err)));
        console.error('Booking page error:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [id]);

  const handleSeatClick = (row, seat) => {  
    const isBooked = occupiedSeats.some(bookedSeat => 
      bookedSeat.row === row && bookedSeat.seat === seat
    );

    if (isBooked) return;

    const totalTickets = selectedTickets.reduce((sum, ticket) => sum + ticket.quantity, 0);
    
    const seatIndex = selectedSeats.findIndex(s => s.row === row && s.seat === seat);
    
    if (seatIndex > -1) {
      setSelectedSeats(selectedSeats.filter((_, index) => index !== seatIndex));
    } else {
      if (selectedSeats.length < totalTickets) {
        setSelectedSeats([...selectedSeats, { row, seat }]);
      } else {
        alert(`Csak ${totalTickets} ülőhelyet választhatsz ki. Ha többet szeretnél, növeld a jegyek számát.`);
      }
    }
  };

  const handleTicketChange = (index, field, value) => {
    const newTickets = [...selectedTickets];
    newTickets[index] = {
      ...newTickets[index],
      [field]: field === 'quantity' ? parseInt(value, 10) : value
    };
    setSelectedTickets(newTickets);
       
    const totalTickets = newTickets.reduce((sum, ticket) => sum + ticket.quantity, 0);
    if (totalTickets < selectedSeats.length) {
      setSelectedSeats([]);
    }
  };

  const addTicketType = () => { 
    const usedTypes = selectedTickets.map(ticket => ticket.type);
    const availableType = ticketTypes.find(type => !usedTypes.includes(type.name));
    
    if (availableType) {
      setSelectedTickets([...selectedTickets, { type: availableType.name, quantity: 1 }]);
    } else {
      alert('Már minden jegytípus hozzá lett adva.');
    }
  };

  const removeTicketType = (index) => {
    if (selectedTickets.length > 1) {
      setSelectedTickets(selectedTickets.filter((_, i) => i !== index));
      setSelectedSeats([]);
    }
  };

  const calculateTotalPrice = () => {
    let total = 0;
    selectedTickets.forEach(ticket => {
      const ticketType = ticketTypes.find(t => t.name === ticket.type);
      if (ticketType) {
        total += basePrice * ticketType.price_multiplier * ticket.quantity;
      }
    });
    return total;
  };

  const calculateTotalTickets = () => {
    return selectedTickets.reduce((sum, ticket) => sum + ticket.quantity, 0);
  };

  const renderSeatGrid = () => {
    const grid = [];
    
    grid.push(
      <div className="screen-container mb-4" key="screen">
        <div className="screen">
          <div className="screen-text">Vetítővászon</div>
        </div>
      </div>
    );
    
    const rowLabels = [];
    for (let row = 1; row <= room.rows; row++) {
      rowLabels.push(
        <div className="seat-row-label" key={`row-label-${row}`}>
          {String.fromCharCode(64 + row)}
        </div>
      );
    }
    grid.push(<div className="row-labels" key="row-labels">{rowLabels}</div>);
    
    const seatContainer = [];
    for (let row = 1; row <= room.rows; row++) {
      const seats = [];
      
      for (let seat = 1; seat <= room.seats_per_row; seat++) {
        const isOccupied = occupiedSeats.some(
          (bookedSeat) => bookedSeat.row === row && bookedSeat.seat === seat
        );
        
        const isSelected = selectedSeats.some(
          (selectedSeat) => selectedSeat.row === row && selectedSeat.seat === seat
        );
        
        seats.push(
          <div 
            key={`seat-${row}-${seat}`}
            className={`seat ${isOccupied ? 'occupied' : ''} ${isSelected ? 'selected' : ''}`}
            onClick={() => handleSeatClick(row, seat)}
            title={`Sor: ${String.fromCharCode(64 + row)}, Szék: ${seat}`}
          >
            {seat}
          </div>
        );
      }
      
      seatContainer.push(
        <div className="seat-row" key={`row-${row}`}>
          {seats}
        </div>
      );
    }
    
    grid.push(<div className="seat-container" key="seat-container">{seatContainer}</div>);
    
    grid.push(
      <div className="seat-legend mt-4" key="legend">
        <div className="d-flex justify-content-center">
          <div className="legend-item me-4">
            <div className="seat-example available"></div>
            <span>Szabad</span>
          </div>
          <div className="legend-item me-4">
            <div className="seat-example selected"></div>
            <span>Kiválasztott</span>
          </div>
          <div className="legend-item">
            <div className="seat-example occupied"></div>
            <span>Foglalt</span>
          </div>
        </div>
      </div>
    );

    return grid;
  };
  
  const formatDate = (dateString, timeString) => {
    if (!dateString) return 'N/A';
    
    try {
      if (typeof dateString === 'string') {
        const dateParts = dateString.split('-');
        if (dateParts.length === 3) {
          const year = parseInt(dateParts[0]);
          const month = parseInt(dateParts[1]) - 1;
          const day = parseInt(dateParts[2]);
          
          if (!isNaN(year) && !isNaN(month) && !isNaN(day)) {
            const date = new Date(year, month, day);
            
            const options = { year: 'numeric', month: 'long', day: 'numeric' };
            const formattedDate = date.toLocaleDateString('hu-HU', options);
          
            if (timeString) {
              return `${formattedDate}, ${timeString}`;
            }
            
            return formattedDate;
          }
        }
      }
      
      return new Date(dateString).toLocaleDateString('hu-HU', { 
        year: 'numeric', month: 'long', day: 'numeric'
      });
    } catch (e) {
      console.error('Date formatting error:', e, dateString);
      return String(dateString);
    }
  };
  
  const nextStep = () => {
    if (step === 1) {
      const totalTickets = selectedTickets.reduce((sum, ticket) => sum + ticket.quantity, 0);
      if (totalTickets <= 0) {
        alert('Legalább egy jegyet kell választanod!');
        return;
      }
    } else if (step === 2) {
      const totalTickets = selectedTickets.reduce((sum, ticket) => sum + ticket.quantity, 0);
      if (selectedSeats.length !== totalTickets) {
        alert(`Pontosan ${totalTickets} ülőhelyet kell kiválasztanod!`);
        return;
      }
    }
    
    setStep(step + 1);
  };
  
  const prevStep = () => {
    setStep(step - 1);
  };
  
  const handleBooking = async () => {
    try {
      if (isScreeningInPast(screening)) {
        setError('Nem lehet jegyet foglalni múltbeli vetítésekre. Kérjük, válasszon egy jövőbeli vetítést.');
        return;
      }
      
      const totalTickets = selectedTickets.reduce((sum, ticket) => sum + ticket.quantity, 0);
      if (selectedSeats.length !== totalTickets) {
        alert(`Pontosan ${totalTickets} ülőhelyet kell kiválasztanod!`);
        return;
      }
       
      const bookingData = {
        screening_id: parseInt(id),
        seats: selectedSeats,
        ticket_types: selectedTickets
      };
      
      setLoading(true);
      const result = await BookingService.createBooking(bookingData);
      alert('Sikeres foglalás!');
      navigate('/bookings');
    } catch (err) {
      setError('Foglalási hiba: ' + (err.message || String(err)));
      console.error('Booking error:', err);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <Container className="d-flex justify-content-center align-items-center" style={{ minHeight: '60vh' }}>
        <div className="text-center">
          <div className="spinner-border text-primary mb-3" role="status">
            <span className="visually-hidden">Loading...</span>
          </div>
          <p className="text-muted">Vetítés adatok betöltése...</p>
        </div>
      </Container>
    );
  }

  if (error) {
    return (
      <Container className="my-5">
        <Card className="border-0 shadow-sm">
          <Card.Body className="p-4">
            <div className="text-center mb-4">
              <i className="bi bi-exclamation-triangle-fill text-danger" style={{ fontSize: '3rem' }}></i>
              <h2 className="mt-3">Hiba történt</h2>
              <p className="lead text-muted">{error}</p>
            </div>
            <div className="d-flex justify-content-center">
              <Button 
                variant="primary" 
                onClick={() => navigate(-1)}
                className="me-3"
              >
                <i className="bi bi-arrow-left me-2"></i>
                Vissza
              </Button>
              <Button 
                variant="outline-secondary" 
                onClick={() => navigate('/')}
              >
                <i className="bi bi-house me-2"></i>
                Főoldal
              </Button>
            </div>
          </Card.Body>
        </Card>
      </Container>
    );
  }

  if (!screening || !movie || !room) {
    return (
      <Container className="my-5">
        <Card className="border-0 shadow-sm">
          <Card.Body className="p-4">
            <div className="text-center mb-4">
              <i className="bi bi-question-circle text-warning" style={{ fontSize: '3rem' }}></i>
              <h2 className="mt-3">Hiányzó adatok</h2>
              <p className="lead text-muted">A foglaláshoz szükséges adatok nem elérhetőek.</p>
            </div>
            <div className="d-flex justify-content-center">
              <Button 
                variant="primary" 
                onClick={() => navigate(-1)}
                className="me-3"
              >
                <i className="bi bi-arrow-left me-2"></i>
                Vissza
              </Button>
              <Button 
                variant="outline-secondary" 
                onClick={() => navigate('/')}
              >
                <i className="bi bi-house me-2"></i>
                Főoldal
              </Button>
            </div>
          </Card.Body>
        </Card>
      </Container>
    );
  }

  return (
    <Container fluid="lg" className="my-5">
      {/* Booking Steps */}
      <div className="booking-stepper mb-4">
        <div className="step-item">
          <div className={`step-number ${step >= 1 ? 'active' : ''} ${step > 1 ? 'completed' : ''}`}>
            {step > 1 ? <i className="bi bi-check-lg"></i> : 1}
          </div>
          <div className="step-title">Jegytípusok</div>
        </div>
        <div className="step-item">
          <div className={`step-number ${step >= 2 ? 'active' : ''} ${step > 2 ? 'completed' : ''}`}>
            {step > 2 ? <i className="bi bi-check-lg"></i> : 2}
          </div>
          <div className="step-title">Ülőhelyek</div>
        </div>
        <div className="step-item">
          <div className={`step-number ${step >= 3 ? 'active' : ''}`}>
            3
          </div>
          <div className="step-title">Összegzés</div>
        </div>
      </div>

      <Row>
        <Col lg={8} className="mb-4">
          <Card className="border-0 shadow-sm overflow-hidden">
            <Card.Header className="bg-white border-bottom d-flex flex-column flex-md-row justify-content-between align-items-start align-items-md-center py-3 gap-2">
              <h5 className="mb-0">{movie?.title}</h5>
              <div className="text-muted">
                <i className="bi bi-calendar-event me-2"></i>
                {screening?.date ? formatDate(screening.date, screening.start_time) : 'N/A'}
              </div>
            </Card.Header>
            <Card.Body>
              {step === 1 && (
                <div>
                  <h4 className="mb-4">
                    <i className="bi bi-ticket-perforated-fill me-2"></i>
                    Jegytípusok kiválasztása
                  </h4>
                  {selectedTickets.map((ticket, index) => (
                    <div key={index} className="mb-3 p-3 bg-light rounded">
                      <div className="d-flex flex-column flex-md-row justify-content-between align-items-stretch gap-3">
                        <Form.Group className="mb-0 flex-grow-1">
                          <Form.Label className="mb-2">Jegytípus</Form.Label>
                          <Form.Select
                            value={ticket.type}
                            onChange={(e) => handleTicketChange(index, 'type', e.target.value)}
                          >
                            {ticketTypes.map((type) => (
                              <option key={type.name} value={type.name}>
                                {type.display_name}
                              </option>
                            ))}
                          </Form.Select>
                        </Form.Group>
                        
                        <Form.Group className="mb-0" style={{ minWidth: '120px' }}>
                          <Form.Label className="mb-2">Mennyiség</Form.Label>
                          <Form.Control
                            type="number"
                            min="1"
                            max="10"
                            value={ticket.quantity}
                            onChange={(e) => handleTicketChange(index, 'quantity', e.target.value)}
                          />
                        </Form.Group>
                        
                        {selectedTickets.length > 1 && (
                          <div className="d-flex align-items-end">
                            <Button 
                              variant="outline-danger" 
                              className="mb-0"
                              onClick={() => removeTicketType(index)}
                            >
                              <i className="bi bi-trash"></i>
                              <span className="d-none d-md-inline ms-1">Törlés</span>
                            </Button>
                          </div>
                        )}
                      </div>
                    </div>
                  ))}
                  
                  {selectedTickets.length < ticketTypes.length && (
                    <Button 
                      variant="outline-primary" 
                      onClick={addTicketType}
                      className="mb-4"
                    >
                      <i className="bi bi-plus-circle me-2"></i>
                      Új jegytípus hozzáadása
                    </Button>
                  )}
                  
                  <div className="alert alert-info">
                    <div className="d-flex">
                      <i className="bi bi-info-circle-fill me-2 fs-5"></i>
                      <div>
                        <strong>Jegyek összesen:</strong> {calculateTotalTickets()}
                        <div className="small text-muted">A következő lépésben ki kell választanod ennyi ülőhelyet.</div>
                      </div>
                    </div>
                  </div>
                </div>
              )}
              
              {step === 2 && (
                <div>
                  <h4 className="mb-4">
                    <i className="bi bi-grid-3x3 me-2"></i>
                    Ülőhely kiválasztása
                  </h4>
                  <div className="alert alert-info mb-4">
                    <div className="d-flex">
                      <i className="bi bi-info-circle-fill me-2 fs-5"></i>
                      <div>
                        <strong>Terem:</strong> {room.name}
                        <div className="small text-muted">Kérjük, válassz ki {calculateTotalTickets()} ülőhelyet.</div>
                      </div>
                    </div>
                  </div>
                  
                  <div className="seat-grid-container mb-4">
                    {renderSeatGrid()}
                  </div>
                  
                  <div className="alert alert-warning mt-4">
                    <div className="d-flex">
                      <i className="bi bi-exclamation-triangle-fill me-2 fs-5"></i>
                      <div>
                        <strong>Kiválasztott ülőhelyek:</strong> {selectedSeats.length} / {calculateTotalTickets()}
                        {selectedSeats.length > 0 && (
                          <div className="mt-2">
                            {selectedSeats.map((seat, index) => (
                              <Badge 
                                bg="primary" 
                                key={index} 
                                className="me-2 mb-2 p-2"
                              >
                                {String.fromCharCode(64 + seat.row)}{seat.seat}
                              </Badge>
                            ))}
                          </div>
                        )}
                      </div>
                    </div>
                  </div>
                </div>
              )}
              
              {step === 3 && (
                <div>
                  <h4 className="mb-4">
                    <i className="bi bi-receipt me-2"></i>
                    Foglalás összegzése
                  </h4>
                  
                  <div className="p-3 bg-light rounded mb-4">
                    <h5 className="border-bottom pb-2 mb-3">Vetítés adatai</h5>
                    <Row className="mb-4">
                      <Col md={6}>
                        <div className="mb-3">
                          <strong>Időpont:</strong> {screening.date ? formatDate(screening.date, screening.start_time) : 'N/A'}
                        </div>
                        <div className="mb-3">
                          <strong>Hossz:</strong> {movie.duration} perc
                        </div>
                      </Col>
                    </Row>
                    
                    <h5 className="border-bottom pb-2 mb-3">Jegyek</h5>
                    <Table className="mb-4">
                      <thead>
                        <tr>
                          <th>Jegytípus</th>
                          <th>Mennyiség</th>
                          <th className="text-end">Ár</th>
                          <th className="text-end">Összesen</th>
                        </tr>
                      </thead>
                      <tbody>
                        {selectedTickets.map((ticket, index) => {
                          const ticketType = ticketTypes.find(t => t.name === ticket.type);
                          const unitPrice = basePrice * (ticketType?.price_multiplier || 1);
                          const subtotal = unitPrice * ticket.quantity;
                          
                          return (
                            <tr key={index}>
                              <td>{ticketType?.display_name || ticket.type}</td>
                              <td>{ticket.quantity}</td>
                              <td className="text-end">{unitPrice.toLocaleString()} Ft</td>
                              <td className="text-end">{subtotal.toLocaleString()} Ft</td>
                            </tr>
                          );
                        })}
                      </tbody>
                      <tfoot>
                        <tr className="fw-bold">
                          <td colSpan="3" className="text-end">Végösszeg:</td>
                          <td className="text-end">{calculateTotalPrice().toLocaleString()} Ft</td>
                        </tr>
                      </tfoot>
                    </Table>
                    
                    <h5 className="border-bottom pb-2 mb-3">Ülőhelyek</h5>
                    <div className="mb-3">
                      {selectedSeats.length > 0 ? (
                        <div className="d-flex flex-wrap">
                          {selectedSeats.map((seat, index) => (
                            <Badge 
                              bg="primary" 
                              key={index} 
                              className="me-2 mb-2 p-2"
                            >
                              {String.fromCharCode(64 + seat.row)}{seat.seat}
                            </Badge>
                          ))}
                        </div>
                      ) : (
                        <div className="text-danger">Nincsenek kiválasztott ülőhelyek!</div>
                      )}
                    </div>
                  </div>
                </div>
              )}
            </Card.Body>
          </Card>
        </Col>
        
        <Col lg={4}>
          <Card className="border-0 shadow-sm booking-summary-card">
            <Card.Body>
              <h4 className="border-bottom pb-3 mb-3">
                <i className="bi bi-info-circle me-2"></i>
                Összegzés
              </h4>
              <div className="mb-3">
                <div className="text-muted small">Film</div>
                <div className="fw-bold">{movie?.title}</div>
              </div>
              <div className="mb-3">
                <div className="text-muted small">Időpont</div>
                <div className="fw-bold">{screening?.date ? formatDate(screening.date, screening.start_time) : 'N/A'}</div>
              </div>
              <div className="mb-3">
                <div className="text-muted small">Jegyek</div>
                <div className="fw-bold">{calculateTotalTickets()} db</div>
              </div>
              <div className="mb-4">
                <div className="text-muted small">Ülőhelyek</div>
                <div className="fw-bold">{selectedSeats.length} / {calculateTotalTickets()} kiválasztva</div>
              </div>
              <div className="d-flex justify-content-between align-items-center mb-4">
                <div className="text-muted">Végösszeg:</div>
                <div className="fs-4 fw-bold">{calculateTotalPrice().toLocaleString()} Ft</div>
              </div>
              
              <div className="d-grid gap-2">
                {step > 1 && (
                  <Button 
                    variant="outline-secondary" 
                    onClick={prevStep}
                  >
                    <i className="bi bi-arrow-left me-2"></i>
                    Vissza
                  </Button>
                )}
                
                {step < 3 ? (
                  <Button 
                    variant="primary" 
                    onClick={nextStep}
                    disabled={
                      (step === 1 && calculateTotalTickets() === 0) ||
                      (step === 2 && selectedSeats.length !== calculateTotalTickets())
                    }
                  >
                    Tovább
                    <i className="bi bi-arrow-right ms-2"></i>
                  </Button>
                ) : (
                  <Button 
                    variant="success" 
                    onClick={handleBooking}
                    disabled={selectedSeats.length !== calculateTotalTickets()}
                  >
                    <i className="bi bi-check-circle me-2"></i>
                    Foglalás véglegesítése
                  </Button>
                )}
              </div>
            </Card.Body>
          </Card>
        </Col>
      </Row>
    </Container>
  );
};

export default BookingPage; 