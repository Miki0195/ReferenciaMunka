import api from './api';
import RoomService from './roomService';
import MovieService from './movieService';

const ScreeningService = {
  getAllScreenings: async () => {
    try {
      
      const roomsResponse = await RoomService.getAllRooms();
      const rooms = Array.isArray(roomsResponse) ? roomsResponse : [];
      
      const response = await api.get('/screenings');
      const screeningsData = response.data.data || response.data || [];

      const formattedScreenings = Array.isArray(screeningsData) 
        ? screeningsData.map(screening => {
            
            const room = rooms.find(r => r.id === screening.room_id);
            
            return {
              ...screening,
              
              week_number: screening.week_number ? parseInt(screening.week_number) : null,
              week_day: screening.week_day ? parseInt(screening.week_day) : null,
              
              room: screening.room || room || { name: 'Ismeretlen terem' }
            };
          })
        : [];
        
      return formattedScreenings;
    } catch (error) {
      console.error('Screening service error:', error);
      throw error.response?.data || error;
    }
  },

  getScreening: async (id) => {
    try {
      let movieData = null;
      let screeningData = null;
      
      try {
        
        const moviesResponse = await api.get('/movies');
        const movies = moviesResponse.data.data || moviesResponse.data || [];
        
        
        for (const movie of movies) {
          if (movie.screenings && Array.isArray(movie.screenings)) {
            const foundScreening = movie.screenings.find(screening => screening.id === parseInt(id));
            if (foundScreening) {
              movieData = movie;
              screeningData = foundScreening;
              console.log('Found screening in movie:', movie.title, 'Screening:', foundScreening);
              break;
            }
          }
        }
        
        
        if (!screeningData) {
          for (const movie of movies) {
            try {
              const movieWithScreeningsResponse = await MovieService.getMovieWithScreenings(movie.id);
              if (movieWithScreeningsResponse && movieWithScreeningsResponse.screenings) {
                const foundScreening = movieWithScreeningsResponse.screenings.find(screening => screening.id === parseInt(id));
                if (foundScreening) {
                  movieData = movieWithScreeningsResponse;
                  screeningData = foundScreening;
                  console.log('Found screening in detailed movie data:', movieWithScreeningsResponse.title, 'Screening:', foundScreening);
                  break;
                }
              }
            } catch (movieError) {
              console.warn(`Failed to get detailed data for movie ${movie.id}:`, movieError);
            }
          }
        }
      } catch (moviesError) {
        console.error('Failed to search for screening in movies:', moviesError);
      }
      
      
      if (!screeningData) {
        const screeningResponse = await api.get(`/screenings/${id}`);
        console.log('Fallback: API Response for screening details:', screeningResponse);
        screeningData = screeningResponse.data.data || screeningResponse.data;
        
        if (!screeningData) {
          throw new Error('Screening data not found');
        }
        
        console.warn('Using screening API fallback - movie data will be incomplete');
      }
      
      let roomData = screeningData.room;
      
      
      if (!roomData || !roomData.rows || (!roomData.seats_per_row && !roomData.seatsPerRow)) {
        try {
          if (screeningData.room_id) {
            console.log(`Fetching room with ID: ${screeningData.room_id}`);
            const roomResponse = await RoomService.getRoom(screeningData.room_id);
            roomData = roomResponse;
            console.log('Room data:', roomData);
          }
        } catch (roomError) {
          console.error('Failed to fetch room data:', roomError);
        }
      }
      
      
      if (!roomData || !roomData.rows || (!roomData.seats_per_row && !roomData.seatsPerRow)) {
        console.warn('Using placeholder room data');
        roomData = {
          id: screeningData.room_id || 0,
          name: roomData?.name || 'Ismeretlen terem',
          rows: roomData?.rows || 10,
          seats_per_row: roomData?.seats_per_row || roomData?.seatsPerRow || 10
        };
      }
      
      
      roomData = {
        ...roomData,
        rows: parseInt(roomData.rows || 10),
        seats_per_row: parseInt(roomData.seats_per_row || roomData.seatsPerRow || 10)
      };
      
      
      const formattedScreening = {
        id: screeningData.id,
        movie_id: movieData.id,
        room_id: screeningData.room_id || roomData.id || 0,
        date: screeningData.date || new Date().toISOString().split('T')[0],
        start_time: screeningData.start_time || '12:00',
        week_number: screeningData.week_number ? parseInt(screeningData.week_number) : null,
        week_day: screeningData.week_day ? parseInt(screeningData.week_day) : null,
        movie: movieData,
        room: roomData,
        
        bookings: screeningData.bookings || []
      };
      
      console.log('Formatted screening data:', formattedScreening);
      return formattedScreening;
    } catch (error) {
      console.error('Error fetching screening details:', error);
      throw error.response?.data || error;
    }
  },

  createScreening: async (screeningData) => {
    try {
      const response = await api.post('/screenings', screeningData);
      return response.data.data || response.data;
    } catch (error) {
      throw error.response?.data || error;
    }
  },

  updateScreening: async (id, screeningData) => {
    try {
      const response = await api.put(`/screenings/${id}`, screeningData);
      const result = response.data.data || response.data;
      
      return result;
    } catch (error) {
      
      if (error.response?.data) {
        throw error.response.data;
      } else if (error.response) {
        throw { message: `HTTP ${error.response.status}: ${error.response.statusText}` };
      } else if (error.message) {
        throw { message: error.message };
      } else {
        throw { message: 'Unknown error occurred while updating screening' };
      }
    }
  },

  deleteScreening: async (id) => {
    try {
      const response = await api.delete(`/screenings/${id}`);
      return response.data.data || response.data;
    } catch (error) {
      throw error.response?.data || error;
    }
  }
};

export default ScreeningService; 