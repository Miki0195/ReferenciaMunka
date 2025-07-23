import { useState, useEffect } from 'react';
import { Form, Button, Container, Card, Alert, Badge } from 'react-bootstrap';
import { useNavigate, useParams } from 'react-router-dom';
import ScreeningService from '../services/screeningService';
import MovieService from '../services/movieService';
import RoomService from '../services/roomService';

const EditScreeningPage = () => {
  const [formData, setFormData] = useState({
    movie_id: '',
    room_id: '',
    start_time: '',
    date: ''
  });
  
  const [movies, setMovies] = useState([]);
  const [rooms, setRooms] = useState([]);
  const [originalScreening, setOriginalScreening] = useState(null);
  const [hasExistingBookings, setHasExistingBookings] = useState(false);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [fetchLoading, setFetchLoading] = useState(true);
  
  const navigate = useNavigate();
  const { id } = useParams();

  useEffect(() => {
    const fetchData = async () => {
      setFetchLoading(true);
      try {
        const [moviesData, roomsData, screeningData] = await Promise.all([
          MovieService.getAllMovies(),
          RoomService.getAllRooms(),
          ScreeningService.getScreening(id)
        ]);
        
        setMovies(moviesData);
        setRooms(roomsData);
        setOriginalScreening(screeningData);
        
        const bookingsExist = screeningData.bookings && 
                             Array.isArray(screeningData.bookings) && 
                             screeningData.bookings.length > 0;
        setHasExistingBookings(bookingsExist);
        
        if (screeningData) {
          let formattedTime = '';
          if (screeningData.start_time) {
            if (screeningData.start_time.includes('T')) {
              const timeParts = screeningData.start_time.split('T')[1].split(':');
              formattedTime = `${timeParts[0]}:${timeParts[1]}`;
            } else if (screeningData.start_time.includes(':')) {
              const timeParts = screeningData.start_time.split(':');
              formattedTime = `${timeParts[0]}:${timeParts[1]}`;
            } else {
              formattedTime = screeningData.start_time;
            }
          }
          
          let validRoomId = screeningData.room_id || '';
          if (validRoomId === 0 || validRoomId === '0') {
            validRoomId = roomsData.length > 0 ? roomsData[0].id : '';
            console.warn(`Original room_id was 0, using default room: ${validRoomId}`);
          }
          
          setFormData({
            movie_id: screeningData.movie_id || '',
            room_id: validRoomId,
            start_time: formattedTime,
            date: screeningData.date || ''
          });
        }
      } catch (err) {
        setError('Failed to fetch data: ' + (err.message || String(err)));
        console.error(err);
      } finally {
        setFetchLoading(false);
      }
    };

    fetchData();
  }, [id]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData({ ...formData, [name]: value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    let formattedTime = formData.start_time;
    if (formattedTime && formattedTime.length > 5) {
      formattedTime = formattedTime.substring(0, 5);
    }

    const timeRegex = /^([01]?[0-9]|2[0-3]):[0-5][0-9]$/;
    if (!timeRegex.test(formattedTime)) {
      setError('Érvénytelen időformátum. Kérjük használj HH:MM formátumot.');
      setLoading(false);
      return;
    }

    const dateRegex = /^\d{4}-\d{2}-\d{2}$/;
    if (!dateRegex.test(formData.date)) {
      setError('Érvénytelen dátumformátum. Kérjük használj YYYY-MM-DD formátumot.');
      setLoading(false);
      return;
    }

    const movieId = parseInt(formData.movie_id);
    let roomId = parseInt(formData.room_id);
    
    if (hasExistingBookings && originalScreening && originalScreening.room_id && originalScreening.room_id > 0) {
      roomId = parseInt(originalScreening.room_id);
    }
    
    if (isNaN(roomId) || roomId <= 0) {
      if (rooms.length > 0) {
        roomId = rooms[0].id;
        console.log(`Fallback: Using first available room_id ${roomId}`);
      } else {
        setError('Nincsenek elérhető termek.');
        setLoading(false);
        return;
      }
    }
    
    console.log('Final roomId:', roomId);
    console.log('roomId validation - isNaN:', isNaN(roomId), 'roomId <= 0:', roomId <= 0);
    
    if (isNaN(movieId) || movieId <= 0) {
      setError('Érvénytelen film kiválasztás.');
      setLoading(false);
      return;
    }
    
    if (isNaN(roomId) || roomId <= 0) {
      setError('Érvénytelen terem kiválasztás.');
      setLoading(false);
      return;
    }

    const screeningData = {
      movie_id: movieId,
      room_id: roomId,
      start_time: formattedTime,
      date: formData.date
    };

    try {
      const dateObj = new Date(formData.date);
      
      const tempDate = new Date(dateObj.getTime());
      tempDate.setDate(tempDate.getDate() + 4 - (tempDate.getDay() || 7));
      const yearStart = new Date(tempDate.getFullYear(), 0, 1);
      const weekNumber = Math.ceil((((tempDate - yearStart) / 86400000) + 1) / 7);
      
      const weekDay = dateObj.getDay() || 7;
      
      screeningData.week_number = weekNumber;
      screeningData.week_day = weekDay;
      
    } catch (dateError) {
      console.error('Error calculating week data:', dateError);
    }

    try {      
      const result = await ScreeningService.updateScreening(id, screeningData);
      navigate('/', { state: { message: 'Vetítés sikeresen frissítve!' } });
    } catch (err) {
      let errorMessage = 'Failed to update screening';
      
      if (err.message && typeof err.message === 'string') {
        errorMessage = err.message;
      } else if (err.errors && typeof err.errors === 'object') {
        const errorMessages = Object.values(err.errors).flat();
        errorMessage = errorMessages.join(', ');
      } else if (err.data && err.data.message) {
        errorMessage = err.data.message;
      } else if (typeof err === 'string') {
        errorMessage = err;
      }
      
      setError(`Frissítési hiba: ${errorMessage}`);
    } finally {
      setLoading(false);
    }
  };

  if (fetchLoading) {
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

  return (
    <Container>
      <Card className="mx-auto" style={{ maxWidth: '800px' }}>
        <Card.Body>
          <h1 className="text-center mb-4">
            <i className="bi bi-pencil-square me-2"></i>
            Vetítés szerkesztése
          </h1>
          
          {error && (
            <Alert variant="danger">
              <i className="bi bi-exclamation-triangle me-2"></i>
              {error}
            </Alert>
          )}
          
          {hasExistingBookings && (
            <Alert variant="info">
              <div className="d-flex align-items-center">
                <i className="bi bi-info-circle-fill me-2 fs-5"></i>
                <div>
                  <strong>Figyelem:</strong> Ehhez a vetítéshez már vannak foglalások. 
                  A terem nem módosítható a foglalások érvényessége miatt.
                  <br />
                  <small>Módosítható: film, dátum, kezdési idő</small>
                </div>
              </div>
            </Alert>
          )}
          
          <Form onSubmit={handleSubmit}>
            <Form.Group className="mb-3" controlId="movie_id">
              <Form.Label>
                <i className="bi bi-film me-2"></i>
                Film
              </Form.Label>
              <Form.Select 
                name="movie_id"
                value={formData.movie_id}
                onChange={handleChange}
                required
              >
                <option value="">Válassz filmet</option>
                {movies.map(movie => (
                  <option key={movie.id} value={movie.id}>
                    {movie.title}
                  </option>
                ))}
              </Form.Select>
            </Form.Group>
            
            <Form.Group className="mb-3" controlId="room_id">
              <Form.Label>
                <i className="bi bi-door-open me-2"></i>
                Terem
                {hasExistingBookings && (
                  <Badge bg="warning" className="ms-2">
                    <i className="bi bi-exclamation-triangle me-1"></i>
                    Foglalások miatt nem módosítható
                  </Badge>
                )}
              </Form.Label>
              <Form.Select 
                name="room_id"
                value={formData.room_id}
                onChange={handleChange}
                disabled={hasExistingBookings}
                required
              >
                <option value="">Válassz termet</option>
                {rooms.map(room => (
                  <option key={room.id} value={room.id}>
                    {room.name} ({room.rows} sor, {room.seats_per_row} szék/sor)
                  </option>
                ))}
              </Form.Select>
              {hasExistingBookings && (
                <Form.Text className="text-muted">
                  <i className="bi bi-info-circle me-1"></i>
                  A terem nem módosítható, mert már vannak foglalások ehhez a vetítéshez.
                </Form.Text>
              )}
            </Form.Group>
            
            <div className="row">
              <div className="col-md-6">
                <Form.Group className="mb-3" controlId="date">
                  <Form.Label>
                    <i className="bi bi-calendar-event me-2"></i>
                    Dátum
                  </Form.Label>
                  <Form.Control 
                    type="date" 
                    name="date"
                    value={formData.date}
                    onChange={handleChange}
                    required 
                  />
                </Form.Group>
              </div>
              
              <div className="col-md-6">
                <Form.Group className="mb-3" controlId="start_time">
                  <Form.Label>
                    <i className="bi bi-clock me-2"></i>
                    Kezdés időpontja
                  </Form.Label>
                  <Form.Control 
                    type="time" 
                    name="start_time"
                    value={formData.start_time}
                    onChange={handleChange}
                    required 
                  />
                </Form.Group>
              </div>
            </div>

            <div className="d-flex justify-content-between mt-4">
              <Button 
                variant="secondary" 
                onClick={() => navigate('/')}
              >
                <i className="bi bi-arrow-left me-2"></i>
                Vissza
              </Button>
              <Button 
                variant="primary" 
                type="submit"
                disabled={loading}
              >
                {loading ? (
                  <>
                    <div className="spinner-border spinner-border-sm me-2" role="status">
                      <span className="visually-hidden">Loading...</span>
                    </div>
                    Frissítés...
                  </>
                ) : (
                  <>
                    <i className="bi bi-check-circle me-2"></i>
                    Vetítés frissítése
                  </>
                )}
              </Button>
            </div>
          </Form>
        </Card.Body>
      </Card>
    </Container>
  );
};

export default EditScreeningPage; 