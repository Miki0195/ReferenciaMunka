import { useState, useEffect } from 'react';
import { Form, Button, Container, Card, Alert } from 'react-bootstrap';
import { useNavigate, useParams } from 'react-router-dom';
import MovieService from '../services/movieService';

const EditMoviePage = () => {
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    image_path: '',
    duration: '',
    genre: '',
    release_year: '',
  });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [fetchLoading, setFetchLoading] = useState(true);
  
  const navigate = useNavigate();
  const { id } = useParams();

  useEffect(() => {
    const fetchMovie = async () => {
      setFetchLoading(true);
      try {
        const movie = await MovieService.getMovie(id);
        setFormData({
          title: movie.title,
          description: movie.description,
          image_path: movie.image_path,
          duration: movie.duration,
          genre: movie.genre,
          release_year: movie.release_year,
        });
      } catch (err) {
        setError('Failed to fetch movie data');
        console.error(err);
      } finally {
        setFetchLoading(false);
      }
    };

    fetchMovie();
  }, [id]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData({ 
      ...formData, 
      [name]: name === 'duration' || name === 'release_year' ? parseInt(value) || '' : value 
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      await MovieService.updateMovie(id, formData);
      navigate('/', { state: { message: 'Film sikeresen frissítve!' } });
    } catch (err) {
      setError(err.message || 'Failed to update movie');
      console.error(err);
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
          <h1 className="text-center mb-4">Film szerkesztése</h1>
          
          {error && <Alert variant="danger">{error}</Alert>}
          
          <Form onSubmit={handleSubmit}>
            <Form.Group className="mb-3" controlId="title">
              <Form.Label>Cím</Form.Label>
              <Form.Control 
                type="text" 
                name="title"
                value={formData.title}
                onChange={handleChange}
                required 
              />
            </Form.Group>
            
            <Form.Group className="mb-3" controlId="description">
              <Form.Label>Leírás</Form.Label>
              <Form.Control 
                as="textarea" 
                rows={3}
                name="description"
                value={formData.description}
                onChange={handleChange}
                required 
              />
            </Form.Group>
            
            <Form.Group className="mb-3" controlId="image_path">
              <Form.Label>Kép URL</Form.Label>
              <Form.Control 
                type="url" 
                name="image_path"
                value={formData.image_path}
                onChange={handleChange}
                required 
              />
            </Form.Group>
            
            <div className="row">
              <div className="col-md-4">
                <Form.Group className="mb-3" controlId="duration">
                  <Form.Label>Hossz (percben)</Form.Label>
                  <Form.Control 
                    type="number" 
                    name="duration"
                    value={formData.duration}
                    onChange={handleChange}
                    required 
                    min="1"
                  />
                </Form.Group>
              </div>
              
              <div className="col-md-4">
                <Form.Group className="mb-3" controlId="genre">
                  <Form.Label>Műfaj</Form.Label>
                  <Form.Control 
                    type="text" 
                    name="genre"
                    value={formData.genre}
                    onChange={handleChange}
                    required 
                  />
                </Form.Group>
              </div>
              
              <div className="col-md-4">
                <Form.Group className="mb-3" controlId="release_year">
                  <Form.Label>Megjelenés éve</Form.Label>
                  <Form.Control 
                    type="number" 
                    name="release_year"
                    value={formData.release_year}
                    onChange={handleChange}
                    required 
                    min="1900"
                    max={new Date().getFullYear() + 5}
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
                {loading ? 'Mentés...' : 'Mentés'}
              </Button>
            </div>
          </Form>
        </Card.Body>
      </Card>
    </Container>
  );
};

export default EditMoviePage; 