import { useState, useEffect } from 'react';
import { Form, Button, Container, Card, Alert } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import ScreeningService from '../services/screeningService';
import MovieService from '../services/movieService';
import RoomService from '../services/roomService';

const AddScreeningPage = () => {
  const [formData, setFormData] = useState({
    movie_id: '',
    room_id: '',
    start_time: '',
    date: ''
  });
  
  const [movies, setMovies] = useState([]);
  const [rooms, setRooms] = useState([]);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [fetchLoading, setFetchLoading] = useState(true);
  
  const navigate = useNavigate();

  useEffect(() => {
    const fetchData = async () => {
      setFetchLoading(true);
      try {
        const [moviesData, roomsData] = await Promise.all([
          MovieService.getAllMovies(),
          RoomService.getAllRooms()
        ]);
        
        setMovies(moviesData);
        setRooms(roomsData);
        
        if (moviesData.length > 0) {
          setFormData(prev => ({ ...prev, movie_id: moviesData[0].id }));
        }
        
        if (roomsData.length > 0) {
          setFormData(prev => ({ ...prev, room_id: roomsData[0].id }));
        }
      } catch (err) {
        setError('Failed to fetch data');
        console.error(err);
      } finally {
        setFetchLoading(false);
      }
    };

    fetchData();
  }, []);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData({ ...formData, [name]: value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    const screeningData = {
      movie_id: parseInt(formData.movie_id),
      room_id: parseInt(formData.room_id),
      start_time: formData.start_time,
      date: formData.date
    };

    try {
      await ScreeningService.createScreening(screeningData);
      navigate('/');
    } catch (err) {
      setError(err.message || 'Failed to add screening');
    } finally {
      setLoading(false);
    }
  };

  if (fetchLoading) {
    return <Container className="text-center mt-5"><div>Loading...</div></Container>;
  }

  return (
    <Container>
      <Card className="mx-auto" style={{ maxWidth: '800px' }}>
        <Card.Body>
          <h1 className="text-center mb-4">Vetítés hozzáadása</h1>
          
          {error && <Alert variant="danger">{error}</Alert>}
          
          <Form onSubmit={handleSubmit}>
            <Form.Group className="mb-3" controlId="movie_id">
              <Form.Label>Film</Form.Label>
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
              <Form.Label>Terem</Form.Label>
              <Form.Select 
                name="room_id"
                value={formData.room_id}
                onChange={handleChange}
                required
              >
                <option value="">Válassz termet</option>
                {rooms.map(room => (
                  <option key={room.id} value={room.id}>
                    {room.name} ({room.rows} sor, {room.seats_per_row} szék/sor)
                  </option>
                ))}
              </Form.Select>
            </Form.Group>
            
            <div className="row">
              <div className="col-md-6">
                <Form.Group className="mb-3" controlId="date">
                  <Form.Label>Dátum</Form.Label>
                  <Form.Control 
                    type="date" 
                    name="date"
                    value={formData.date}
                    onChange={handleChange}
                    required 
                    min={new Date().toISOString().split('T')[0]}
                  />
                </Form.Group>
              </div>
              
              <div className="col-md-6">
                <Form.Group className="mb-3" controlId="start_time">
                  <Form.Label>Kezdés időpontja</Form.Label>
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
                Vissza
              </Button>
              <Button 
                variant="primary" 
                type="submit"
                disabled={loading}
              >
                {loading ? 'Hozzáadás...' : 'Vetítés hozzáadása'}
              </Button>
            </div>
          </Form>
        </Card.Body>
      </Card>
    </Container>
  );
};

export default AddScreeningPage; 